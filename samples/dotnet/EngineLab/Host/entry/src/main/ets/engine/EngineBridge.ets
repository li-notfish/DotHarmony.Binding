// ArkTS 引擎实验层：引擎桥（C# 经 napi 持有的对象）
// .NET → 引擎：init(onEvent) / applyCommands(cmds)（HarmonyEngineInit 通道 + Flush 批量冲刷）
// 引擎 → .NET：emitClick / emitArea / emitText / tick 经注入的 onEvent 回调回流
// （P2：tick 为 16ms 定时冲刷兜底，C# 在 napi 回调线程内冲刷积压指令）
import { RenderNode } from './EngineTypes';
import { EngineScene } from './EngineScene';
import display from '@ohos.display';

/** 事件回流载荷：量测 px 值 + 密度；textChange 携带文本值 */
export interface EventPayload {
  width: number;
  height: number;
  density: number;
  value: string;
}

export type EngineEventCallback = (id: number, kind: string, payload: EventPayload) => void;

export type TreeListener = (root: RenderNode | null) => void;

/** 自动冲刷周期（与 C# 侧 ArkTsEngine.AutoFlushIntervalMs 对齐） */
const AUTO_FLUSH_MS = 16;

/** tick 回流的 id 哨兵（无对应节点，C# 侧按 kind 先行分流） */
const TICK_ID = -1;

export class EngineBridge {
  private scene: EngineScene = new EngineScene();
  private onEvent: EngineEventCallback | null = null;
  private listener: TreeListener | null = null;
  private density: number = 3.25;

  constructor() {
    try {
      this.density = display.getDefaultDisplaySync().densityPixels;
    } catch (err) {
      // 查询失败仅影响 px 换算精度，保底 3.25
      console.warn(`[EngineBridge] density query failed: ${err}`);
    }
  }

  /** EngineHost 注册渲染快照监听（有变更的 applyCommands 后同步回调） */
  setListener(listener: TreeListener): void {
    this.listener = listener;
  }

  // 以下三个方法由 .NET 侧调用（勿改名：C# MarshalCommand / InitializeCore 依赖）

  /** .NET 注入回流回调（HarmonyEngineInit 内同步调用） */
  init(callback: EngineEventCallback): void {
    this.onEvent = callback;
  }

  /** P2 自动冲刷兜底：16ms 定时发 tick 回流，C# 冲刷积压指令（同帧写合并） */
  enableAutoFlush(intervalMs: number): void {
    setInterval(() => {
      this.onEvent?.(TICK_ID, 'tick', { width: 0, height: 0, density: this.density, value: '' });
    }, intervalMs > 0 ? intervalMs : AUTO_FLUSH_MS);
  }

  /** 整批指令入口：落 SceneGraph → 增量 diff 渲染快照（无变更时不下发）→ 通知监听者 */
  applyCommands(cmds: ESObject[]): void {
    this.scene.apply(cmds);
    const root: RenderNode | null = this.scene.buildTree();
    if (root !== null) {
      this.listener?.(root);
    }
  }

  // 以下由 DynamicNode 组件回调

  emitClick(id: number): void {
    this.onEvent?.(id, 'click', { width: 0, height: 0, density: this.density, value: '' });
  }

  emitArea(id: number, area: Area): void {
    this.onEvent?.(id, 'area', {
      width: this.toPx(area.width),
      height: this.toPx(area.height),
      density: this.density,
      value: '',
    });
  }

  emitText(id: number, value: string): void {
    this.onEvent?.(id, 'textChange', { width: 0, height: 0, density: this.density, value: value });
  }

  /** onAreaChange 给出 vp 值，换算 px（引擎侧密度推导的实验实现） */
  private toPx(v: Length): number {
    return typeof v === 'number' ? v * this.density : 0;
  }
}
