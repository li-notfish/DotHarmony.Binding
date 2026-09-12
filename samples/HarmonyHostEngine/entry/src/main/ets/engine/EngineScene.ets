// ArkTS 引擎实验层：SceneGraph（MIGRATION_ARKTS_ENGINE.md §2.3）
// Map<id, SceneRecord> 保留树；指令先落图，再全量重建渲染快照驱动 @State。
import { NodeAttrs, RenderNode } from './EngineTypes';

/** 引擎侧节点记录：指令的收敛目标 */
class SceneRecord {
  id: number = 0;
  type: string = '';
  attrs: Map<string, ESObject> = new Map<string, ESObject>();
  children: number[] = [];
  events: string[] = [];
}

export class EngineScene {
  private records: Map<number, SceneRecord> = new Map<number, SceneRecord>();
  private rootId: number = -1;
  private rev: number = 0;

  /**
   * 应用一批指令（C# Flush 一次 napi 调用下发）。
   * 未知 id 的操作（节点已删除/未创建）静默忽略——删除级联与乱序指令的容错。
   */
  apply(cmds: ESObject[]): void {
    for (let i = 0; i < cmds.length; i++) {
      const c: ESObject = cmds[i];
      const op: string = c.op;
      const id: number = c.id;
      switch (op) {
        case 'create': {
          const rec = new SceneRecord();
          rec.id = id;
          rec.type = c.type;
          this.records.set(id, rec);
          break;
        }
        case 'setAttrs': {
          const rec = this.records.get(id);
          if (!rec) break;
          const pairs: ESObject[] = c.attrs;
          for (let j = 0; j < pairs.length; j++) {
            const pair: ESObject = pairs[j];
            rec.attrs.set(pair[0] as string, pair[1]);
          }
          break;
        }
        case 'setChildren': {
          const rec = this.records.get(id);
          if (!rec) break;
          rec.children = [];
          const ids: ESObject[] = c.children;
          for (let j = 0; j < ids.length; j++) {
            rec.children.push(ids[j] as number);
          }
          break;
        }
        case 'setEvents': {
          const rec = this.records.get(id);
          if (!rec) break;
          rec.events = [];
          const names: ESObject[] = c.events;
          for (let j = 0; j < names.length; j++) {
            rec.events.push(names[j] as string);
          }
          break;
        }
        case 'delete':
          this.removeRecursive(id);
          break;
        case 'setRoot':
          this.rootId = id;
          break;
        default:
          console.warn(`[EngineScene] unknown op: ${op}`);
          break;
      }
    }
  }

  /** 全量重建渲染快照；无根或根已删除时返回 null（EngineHost 保持等待态） */
  buildTree(): RenderNode | null {
    const root = this.records.get(this.rootId);
    if (!root) {
      return null;
    }
    this.rev++;
    return this.build(root);
  }

  private build(rec: SceneRecord): RenderNode {
    const node = new RenderNode();
    node.id = rec.id;
    node.type = rec.type;
    node.attrs = this.toAttrs(rec.attrs);
    node.events = rec.events;
    node.rev = this.rev;
    for (const childId of rec.children) {
      const child = this.records.get(childId);
      if (child) {
        node.children.push(this.build(child));
      }
    }
    return node;
  }

  private toAttrs(m: Map<string, ESObject>): NodeAttrs {
    const a = new NodeAttrs();
    if (m.has('width')) a.width = m.get('width') as Length;
    if (m.has('height')) a.height = m.get('height') as Length;
    if (m.has('x')) a.x = m.get('x') as number;
    if (m.has('y')) a.y = m.get('y') as number;
    if (m.has('backgroundColor')) a.backgroundColor = m.get('backgroundColor') as ResourceColor;
    if (m.has('opacity')) a.opacity = m.get('opacity') as number;
    if (m.has('text')) a.text = m.get('text') as string;
    if (m.has('fontSize')) a.fontSize = m.get('fontSize') as number;
    if (m.has('fontColor')) a.fontColor = m.get('fontColor') as ResourceColor;
    if (m.has('label')) a.label = m.get('label') as string;
    return a;
  }

  private removeRecursive(id: number): void {
    const rec = this.records.get(id);
    if (!rec) {
      return;
    }
    for (const childId of rec.children) {
      this.removeRecursive(childId);
    }
    this.records.delete(id);
  }
}
