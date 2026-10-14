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
  // 固定 webTag：NDK ArkWeb API（SetJavaScriptProxyValidCallback 等）按 tag 寻址本组件
  private webController: webview.WebviewController = new webview.WebviewController('harmony-web');
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
          native.passWebView(this.webController);
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
        .onPageEnd(() => {
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
