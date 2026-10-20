import { deviceInfo } from '../ohosImports';
import { EngineHost } from '../engine/EngineHost';

// 注意：native 侧经 napi_load_module 动态加载的 @ohos.* 系统模块，
// 统一由 ohosImports.ets 登记（编入 HAP 模块表），由 ArkTsBinding 生成器维护。
// 本页引用 deviceInfo 作为该清单的真实使用点。

/**
 * .NET 宿主页（ArkTS 引擎实验版）：
 * 标题区为 ArkTS 声明式对照，下方 EngineHost 走命令总线引擎——
 * C# (NativeAOT) 发指令流，本页经 DynamicNode 递归渲染真实 ArkUI 组件树。
 * 与共享 C 节点 API 宿主模板（samples/HarmonyHost 的 ContentSlot 路线）互不影响。
 */
@Entry
@Component
struct Index {
  @State message: string = 'ArkTS engine lab (Experimental)';
  @State dotnetStatus: string = 'initializing .NET...';

  aboutToAppear(): void {
    console.info(`[host] arkts deviceInfo: ${deviceInfo.brand} ${deviceInfo.productModel} os=${deviceInfo.osFullName}`);
    this.dotnetStatus = 'engine mount below';
  }

  build() {
    Column() {
      Text(this.message)
        .fontSize(16)
        .fontWeight(FontWeight.Bold)
        .margin({ top: 24 })

      Text(this.dotnetStatus)
        .fontSize(12)
        .fontColor('#666666')
        .margin({ top: 4 })

      // ArkTS 命令总线引擎的渲染区（EngineHost 自行完成 initEngine 通道握手）
      EngineHost()
    }
    .width('100%')
    .height('100%')
    .backgroundColor('#FFFFFF')
  }
}
