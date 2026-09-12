// ArkTS 引擎实验层：递归渲染组件
// 按 type 分派到具体 ArkUI 组件，属性从 NodeAttrs 直映修饰符——
// 这份映射表是引擎仅有的"属性知识"（MIGRATION_ARKTS_ENGINE.md §2.3，
// 正式版应改为声明式配置 mapping.ets，与 NDK 生成器产物共用同一源描述）。
// 事件：onClick/onAreaChange 恒挂接，回流到 C# 后按订阅分派。
import { RenderNode } from './EngineTypes';
import { EngineBridge } from './EngineBridge';

@Component
export struct DynamicNode {
  @Prop node: RenderNode;
  bridge: EngineBridge = new EngineBridge();

  build() {
    if (this.node.type === 'Stack') {
      Column() {
        ForEach(this.node.children, (child: RenderNode) => {
          DynamicNode({ node: child, bridge: this.bridge })
        }, (child: RenderNode) => `${child.id}:${child.rev}`)
      }
      .width(this.node.attrs.width ?? 'auto')
      .height(this.node.attrs.height ?? 'auto')
      .backgroundColor(this.node.attrs.backgroundColor ?? Color.Transparent)
      .opacity(this.node.attrs.opacity ?? 1)
      .onClick(() => this.bridge.emitClick(this.node.id))
      .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Text') {
      Text(this.node.attrs.text ?? '')
        .fontSize(this.node.attrs.fontSize ?? 14)
        .fontColor(this.node.attrs.fontColor ?? '#E0000000')
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Button') {
      Button(this.node.attrs.label ?? '')
        .fontSize(this.node.attrs.fontSize ?? 14)
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    }
  }
}
