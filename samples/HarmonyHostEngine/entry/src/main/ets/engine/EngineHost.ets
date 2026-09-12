// ArkTS 引擎实验层：宿主渲染组件
// 职责：创建引擎桥 → 走 initEngine 通道交给 .NET（C# 侧同步 init(onEvent) + 冲刷积压指令）
// → 监听渲染快照 → @State 驱动 DynamicNode 递归渲染。
// 与共享 C 节点宿主（ContentSlot + passNodeContent）互不影响。
import { EngineBridge } from './EngineBridge';
import { RenderNode } from './EngineTypes';
import { DynamicNode } from './DynamicNode';
import native from 'libentry.so';

@Component
export struct EngineHost {
  @State root: RenderNode | null = null;
  private bridge: EngineBridge = new EngineBridge();

  aboutToAppear(): void {
    this.bridge.setListener((root: RenderNode | null) => {
      this.root = root;
    });
    // C# 通道：存桥引用 → init(onEvent) → 冲刷积压指令 → 同步回调渲染快照
    native.initEngine(this.bridge);
  }

  build() {
    Column() {
      if (this.root) {
        DynamicNode({ node: this.root, bridge: this.bridge })
      } else {
        Text('engine: waiting for commands...')
          .fontSize(12)
          .fontColor('#999999')
      }
    }
    .width('100%')
    .layoutWeight(1)
  }
}
