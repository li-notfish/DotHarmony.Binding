// ArkTS 引擎实验层：递归渲染组件
// 按 type 分派到具体 ArkUI 组件，属性从 NodeAttrs 直映修饰符——
// 这份映射表是引擎仅有的"属性知识"（MIGRATION_ARKTS_ENGINE.md §2.3，
// 正式版应改为声明式配置 mapping.ets，与 NDK 生成器产物共用同一源描述）。
// 事件：onClick/onAreaChange/onChange 恒挂接，回流到 C# 后按订阅分派。
// 增量渲染（P2）：未变更子树的 ForEach key（id:rev）不变，组件由 ArkUI 复用。
import { RenderNode } from './EngineTypes';
import { EngineBridge } from './EngineBridge';

@Component
export struct DynamicNode {
  @Prop node: RenderNode;
  bridge: EngineBridge = new EngineBridge();

  /** 绝对定位（vp）；未设置时传 undefined（ArkUI 修饰符对 undefined 不生效） */
  private positionValue(): Position | undefined {
    if (this.node.attrs.x === undefined && this.node.attrs.y === undefined) {
      return undefined;
    }
    return { x: this.node.attrs.x ?? 0, y: this.node.attrs.y ?? 0 };
  }

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
      .position(this.positionValue())
      .onClick(() => this.bridge.emitClick(this.node.id))
      .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Scroll') {
      Scroll() {
        Column() {
          ForEach(this.node.children, (child: RenderNode) => {
            DynamicNode({ node: child, bridge: this.bridge })
          }, (child: RenderNode) => `${child.id}:${child.rev}`)
        }
        .width('100%')
      }
      .scrollable(ScrollDirection.Vertical)
      .width(this.node.attrs.width ?? 'auto')
      .height(this.node.attrs.height ?? 'auto')
      .backgroundColor(this.node.attrs.backgroundColor ?? Color.Transparent)
      .opacity(this.node.attrs.opacity ?? 1)
      .position(this.positionValue())
      .onClick(() => this.bridge.emitClick(this.node.id))
      .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Text') {
      Text(this.node.attrs.text ?? '')
        .fontSize(this.node.attrs.fontSize ?? 14)
        .fontColor(this.node.attrs.fontColor ?? '#E0000000')
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .position(this.positionValue())
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Entry') {
      Entry({ placeholder: this.node.attrs.placeholder ?? '', text: this.node.attrs.text ?? '' })
        .fontSize(this.node.attrs.fontSize ?? 14)
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .position(this.positionValue())
        .onChange((value: string) => this.bridge.emitText(this.node.id, value))
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Image') {
      Image(this.node.attrs.src ?? '')
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .position(this.positionValue())
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    } else if (this.node.type === 'Button') {
      Button(this.node.attrs.label ?? '')
        .fontSize(this.node.attrs.fontSize ?? 14)
        .width(this.node.attrs.width ?? 'auto')
        .height(this.node.attrs.height ?? 'auto')
        .opacity(this.node.attrs.opacity ?? 1)
        .position(this.positionValue())
        .onClick(() => this.bridge.emitClick(this.node.id))
        .onAreaChange((_oldArea: Area, newArea: Area) => this.bridge.emitArea(this.node.id, newArea))
    }
  }
}
