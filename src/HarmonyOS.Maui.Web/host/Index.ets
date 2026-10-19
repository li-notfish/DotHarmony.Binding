import { webview } from '@kit.ArkWeb';
import { NodeContent } from '@kit.ArkUI';
import native from 'libentry.so';

class ExternalBridge {
  private receiveMessageCallback?: (message: string) => void;

  receiveMessage(callback: (message: string) => void): void {
    this.receiveMessageCallback = callback;
  }

  sendMessage(message: string): void {
    native.receiveWebViewMessage(message);
  }

  dispatchMessage(message: string): void {
    if (this.receiveMessageCallback) {
      this.receiveMessageCallback(message);
    }
  }
}

@Entry
@Component
struct Index {
  private nodeContent: NodeContent = new NodeContent();
  // 固定 webTag：NDK ArkWeb API（SetJavaScriptProxyValidCallback / SetSchemeHandler）按 tag 寻址本组件
  private readonly webTag: string = 'harmony-web';
  private webController: webview.WebviewController = new webview.WebviewController(this.webTag);
  private externalBridge: ExternalBridge = new ExternalBridge();
  private readonly baseUri: string = 'https://localhost/';
  private nodeContentPassed: boolean = false;

  aboutToAppear(): void {
    native.initDotnet();
  }

  build() {
    Stack() {
      Column() {
        ContentSlot(this.nodeContent)
      }
      .hitTestBehavior(HitTestMode.Transparent)
      Web({ src: this.baseUri, controller: this.webController })
        .zIndex(2)
        .width('100%')
        .height('100%')
        .javaScriptAccess(true)
        .domStorageAccess(true)
        .fileAccess(true)
        .onAppear(() => {
          native.passWebView(this.webController, this.webTag);
        })
        .javaScriptProxy({
          object: this.externalBridge,
          name: 'external',
          methodList: ['receiveMessage', 'sendMessage', 'dispatchMessage'],
          controller: this.webController
        })
        .onErrorReceive((event) => {
          console.error(`[host] error receive: ${event.request.getRequestUrl()}`);
        })
        // 导航拦截：外链策略由 .NET 侧 UrlLoading 决策。ArkWeb 语义：返回 true 拦截（不加载）。
        // 注意 napi 返回的是 JS boolean，不能用 === 1 比较（恒 false），否则拦截静默失效。
        .onLoadIntercept((event) => {
          const url = event.data.getRequestUrl();
          return native.shouldInterceptNavigation(url, this.webTag) as boolean;
        })
        .onPageEnd((event) => {
          // 主框架加载完成：Blazor 启动的确定性信号（.NET 侧以此触发 Blazor.start）
          native.notifyWebPageEnded(event.url, this.webTag);

          // NodeContent 只需交接一次；后续页面加载完成不再重发
          if (this.nodeContentPassed) {
            return;
          }
          this.nodeContentPassed = true;
          setTimeout(() => {
            native.passNodeContent(this.nodeContent);
          }, 0);
        })
    }
    .width('100%')
    .height('100%')
  }
}
