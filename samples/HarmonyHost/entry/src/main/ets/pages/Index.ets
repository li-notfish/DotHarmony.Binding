import { NodeContent } from '@kit.ArkUI';
import { deviceInfo } from '../ohosImports';
import native from 'libentry.so';

// 注意：native 侧经 napi_load_module 动态加载的 @ohos.* 系统模块，
// 统一由 ohosImports.ets 登记（编入 HAP 模块表），由 ArkTsBinding 生成器维护。
// 本页引用 deviceInfo 作为该清单的真实使用点。

/**
 * .NET 宿主页：上方为 ArkTS 声明式区域（对照用），
 * ContentSlot 内挂载 .NET (NativeAOT) 经 ArkUI C API 构建的原生节点树。
 */
@Entry
@Component
struct Index {
  @State message: string = 'ArkTS host page';
  @State dotnetStatus: string = 'initializing .NET...';
  private nodeContent: NodeContent = new NodeContent();

  aboutToAppear(): void {
    // ArkTS 侧对照读取（与 .NET 侧经 napi_load_module 的读取结果应一致）
    console.info(`[host] arkts deviceInfo: ${deviceInfo.brand} ${deviceInfo.productModel} os=${deviceInfo.osFullName}`);
    // 1. 初始化 .NET 运行时（dlopen libapp.so + env 注入）
    native.initDotnet();
    // 2. 把 NodeContent 交给 .NET 侧，由其用 ArkUI C API 命令式构建 UI
    native.passNodeContent(this.nodeContent);
    this.dotnetStatus = '.NET UI mounted below';
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

      // .NET 构建的原生节点树挂载点
      Row() {
        ContentSlot(this.nodeContent)
      }
      .width('100%')
      .layoutWeight(1)
    }
    .width('100%')
    .height('100%')
    .backgroundColor('#FFFFFF')
  }
}
