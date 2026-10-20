// ArkTS 引擎实验层：渲染模型（MIGRATION_ARKTS_ENGINE.md §2.3）
// P2 完整版：Image/Entry/Scroll 六类节点；增量 diff（RenderNode.rev 按节点批号）。

/**
 * 节点属性集。C# 侧经 setAttrs 指令以 [name, value] 对下发，
 * EngineScene 收敛到 Map 后在渲染快照重建时转为本类型。
 */
export class NodeAttrs {
  width?: Length;
  height?: Length;
  /** 绝对定位（vp），映射 .position({x,y})——P2 起应用 */
  x?: number;
  y?: number;
  backgroundColor?: ResourceColor;
  opacity?: number;
  text?: string;
  fontSize?: number;
  fontColor?: ResourceColor;
  label?: string;
  /** 仅 Image：图片来源（沙箱路径/网络 URL） */
  src?: string;
  /** 仅 Entry：占位文本 */
  placeholder?: string;
}

/**
 * 渲染快照节点。rev 是"该节点最近一次变更时的引擎批号"（不再是全树批号）：
 * 增量 diff 下只有被指令改动的节点 rev 前进，参与 ForEach key
 * （`${id}:${rev}`），未变更子树的组件由 ArkUI 按 key 复用——P2 增量渲染。
 */
export class RenderNode {
  id: number = 0;
  type: string = '';
  attrs: NodeAttrs = new NodeAttrs();
  children: RenderNode[] = [];
  events: string[] = [];
  rev: number = 0;
}
