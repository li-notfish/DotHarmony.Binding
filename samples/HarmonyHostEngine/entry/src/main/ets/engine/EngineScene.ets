// ArkTS 引擎实验层：SceneGraph（MIGRATION_ARKTS_ENGINE.md §2.3）
// P2 增量 diff：指令先落图，按变更集增量更新渲染缓存（RenderNode 实例原位复用），
// 只有被指令改动的节点 rev 前进——未变更子树由 ArkUI 按 ForEach key 复用组件。
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
  /** 渲染缓存：id → 复用的 RenderNode 实例（未变更节点原位保留） */
  private cache: Map<number, RenderNode> = new Map<number, RenderNode>();
  /** 本批变更集（待重建的节点 id） */
  private dirty: Set<number> = new Set<number>();
  /** childId → parentId（delete 时定位受影响的父节点） */
  private parents: Map<number, number> = new Map<number, number>();
  private rootId: number = -1;
  private rev: number = 0;
  /** 根级变更（setRoot / delete 兜底）——整树重建 */
  private rootDirty: boolean = false;

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
          this.dirty.add(id);
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
          this.dirty.add(id);
          break;
        }
        case 'setChildren': {
          const rec = this.records.get(id);
          if (!rec) break;
          const ids: ESObject[] = c.children;
          const next: number[] = [];
          for (let j = 0; j < ids.length; j++) {
            const childId: number = ids[j] as number;
            next.push(childId);
            this.parents.set(childId, id);
          }
          // 摘除的子节点清父指针（节点可再挂回，缓存实例保留）
          for (const old of rec.children) {
            if (!next.includes(old)) {
              this.parents.delete(old);
            }
          }
          rec.children = next;
          this.dirty.add(id);
          break;
        }
        case 'setEvents': {
          // 只更新记录，不标脏：事件订阅不影响渲染快照（DynamicNode 恒挂接回调，回流按订阅分派）
          const rec = this.records.get(id);
          if (!rec) break;
          const names: ESObject[] = c.events;
          const events: string[] = [];
          for (let j = 0; j < names.length; j++) {
            events.push(names[j] as string);
          }
          rec.events = events;
          break;
        }
        case 'delete':
          this.removeRecursive(id);
          break;
        case 'setRoot':
          this.rootId = id;
          this.rootDirty = true;
          break;
        default:
          console.warn(`[EngineScene] unknown op: ${op}`);
          break;
      }
    }
  }

  /**
   * 增量重建渲染快照。无变更时返回 null（EngineBridge 不下发，避免无谓重渲染）；
   * 有变更时返回根的浅拷贝包装（EngineHost 的 @State 以引用变更感知本轮更新），
   * 子孙节点优先复用缓存实例——rev 未前进的子树由 ArkUI 按 key 复用组件。
   */
  buildTree(): RenderNode | null {
    const rootRec = this.records.get(this.rootId);
    if (!rootRec) {
      return null;
    }
    if (!this.rootDirty && this.dirty.size === 0) {
      return null;
    }
    this.rev++;
    this.rebuild(rootRec);
    const root = this.cache.get(rootRec.id);
    if (!root) {
      return null;
    }
    this.dirty.clear();
    this.rootDirty = false;
    const wrapper = new RenderNode();
    wrapper.id = root.id;
    wrapper.type = root.type;
    wrapper.attrs = root.attrs;
    wrapper.events = root.events;
    wrapper.rev = this.rev;
    wrapper.children = root.children;
    return wrapper;
  }

  /**
   * 增量重建以 rec 为根的子树：脏节点（或新节点）更新缓存实例并推进 rev；
   * 未脏节点整体跳过（其实例经共享引用可见，children 已含变更后代的实例）。
   */
  private rebuild(rec: SceneRecord): void {
    const cached = this.cache.get(rec.id);
    if (cached !== undefined && !this.dirty.has(rec.id)) {
      return;
    }
    let node: RenderNode;
    if (cached !== undefined) {
      node = cached;
    } else {
      node = new RenderNode();
      this.cache.set(rec.id, node);
    }
    node.id = rec.id;
    node.type = rec.type;
    node.attrs = this.toAttrs(rec.attrs);
    node.events = rec.events;
    node.rev = this.rev;
    const children: RenderNode[] = [];
    for (const childId of rec.children) {
      const childRec = this.records.get(childId);
      if (childRec) {
        this.rebuild(childRec);
        const child = this.cache.get(childId);
        if (child) {
          children.push(child);
        }
      }
    }
    node.children = children;
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
    if (m.has('src')) a.src = m.get('src') as string;
    if (m.has('placeholder')) a.placeholder = m.get('placeholder') as string;
    return a;
  }

  private removeRecursive(id: number): void {
    const rec = this.records.get(id);
    if (!rec) {
      return;
    }
    const parent = this.parents.get(id);
    for (const childId of rec.children) {
      this.removeRecursive(childId);
    }
    this.records.delete(id);
    this.cache.delete(id);
    this.parents.delete(id);
    if (parent !== undefined) {
      // 父节点渲染缓存的子表可能残留已删实例，标脏强制其子表重建
      this.dirty.add(parent);
    }
  }
}
