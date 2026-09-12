// ArkTS 引擎实验层：引擎桥（C# 经 napi 持有的对象）
// .NET → 引擎：init(onEvent) / applyCommands(cmds)（HarmonyEngineInit 通道 + Flush 批量冲刷）
// 引擎 → .NET：emitClick / emitArea 经注入的 onEvent 回调回流（事件与量测）
import { RenderNode } from './EngineTypes';
import { EngineScene } from './EngineScene';
import display from '@ohos.display';

/** 量测回流载荷：px 值 + 密度（px/vp，随量测直接下发，替代 C# 侧比值推算） */
export interface AreaPayload {
  width: number;
  height: number;
  density: number;
}

export type EngineEventCallback = (id: number, kind: string, payload: AreaPayload) => void;

export type TreeListener = (root: RenderNode | null) => void;

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

  /** EngineHost 注册渲染快照监听（applyCommands 后同步回调） */
  setListener(listener: TreeListener): void {
    this.listener = listener;
  }

  // 以下两个方法由 .NET 侧调用（勿改名：C# MarshalCommand / InitializeCore 依赖）

  /** .NET 注入回流回调（HarmonyEngineInit 内同步调用） */
  init(callback: EngineEventCallback): void {
    this.onEvent = callback;
  }

  /** 整批指令入口：落 SceneGraph → 全量重建渲染快照 → 通知监听者 */
  applyCommands(cmds: ESObject[]): void {
    this.scene.apply(cmds);
    this.listener?.(this.scene.buildTree());
  }

  // 以下由 DynamicNode 组件回调

  emitClick(id: number): void {
    this.onEvent?.(id, 'click', { width: 0, height: 0, density: this.density });
  }

  emitArea(id: number, area: Area): void {
    this.onEvent?.(id, 'area', {
      width: this.toPx(area.width),
      height: this.toPx(area.height),
      density: this.density,
    });
  }

  /** onAreaChange 给出 vp 值，换算 px（引擎侧密度推导的实验实现） */
  private toPx(v: Length): number {
    return typeof v === 'number' ? v * this.density : 0;
  }
}
