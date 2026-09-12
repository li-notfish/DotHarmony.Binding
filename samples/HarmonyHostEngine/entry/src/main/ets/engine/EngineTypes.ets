// ArkTS 引擎实验层：渲染模型（MIGRATION_ARKTS_ENGINE.md §2.3 的 MVP 子集）
// 实验范围：Stack/Text/Button 三类节点；属性映射表见 DynamicNode（引擎仅有的"映射知识"）。

/**
 * 节点属性集。C# 侧经 setAttrs 指令以 [name, value] 对下发，
 * EngineScene 收敛到 Map 后在渲染快照重建时转为本类型。
 */
export class NodeAttrs {
  width?: Length;
  height?: Length;
  /** 预留：托管布局的绝对定位（实验版存储但不应用，P2 与托管布局收敛时启用） */
  x?: number;
  y?: number;
  backgroundColor?: ResourceColor;
  opacity?: number;
  text?: string;
  fontSize?: number;
  fontColor?: ResourceColor;
  label?: string;
}

/**
 * 渲染快照节点。每次 applyCommands 全量重建，rev（引擎批号）参与 ForEach key，
 * 强制整树刷新——MVP 策略，增量 diff 留给 P2 完整版。
 */
export class RenderNode {
  id: number = 0;
  type: string = '';
  attrs: NodeAttrs = new NodeAttrs();
  children: RenderNode[] = [];
  events: string[] = [];
  rev: number = 0;
}
