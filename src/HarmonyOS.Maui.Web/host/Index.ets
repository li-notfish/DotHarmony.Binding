import { webview } from '@kit.ArkWeb';
import { NodeContent, BuilderNode } from '@kit.ArkUI';
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

/**
 * Web 的 BuilderNode 版本（内嵌模式）：Web 不再由页面 build() 直接声明，
 * 而是包成节点交给 .NET，由 MAUI 布局决定尺寸与位置。
 * 所有 Web 级回调（proxy / 导航拦截 / 页面事件）都挂在 Builder 内的 Web 上。
 */
interface WebNodeArgs {
  controller: webview.WebviewController;
  bridge: ExternalBridge;
  webTag: string;
  onPageEnded: () => void;
  onControllerReady: () => void;
}

@Builder
function WebNodeBuilder(args: WebNodeArgs) {
  Web({ src: 'https://localhost/', controller: args.controller })
    .width('100%')
    .height('100%')
    .javaScriptAccess(true)
    .domStorageAccess(true)
    .fileAccess(true)
    .javaScriptProxy({
      object: args.bridge,
      name: 'external',
      methodList: ['receiveMessage', 'sendMessage', 'dispatchMessage'],
      controller: args.controller
    })
    // WebviewController 与 Web 组件完成关联的精确信号：此时才可调用 registerJavaScriptProxy 等 API。
    .onControllerAttached(() => {
      args.onControllerReady();
    })
    .onErrorReceive((event) => {
      console.error(`[host] error receive: ${event.request.getRequestUrl()}`);
    })
    // 导航拦截：外链策略由 .NET 侧 UrlLoading 决策。ArkWeb 语义：返回 true 拦截（不加载）。
    // 注意 napi 返回的是 JS boolean，不能用 === 1 比较（恒 false），否则拦截静默失效。
    .onLoadIntercept((event) => {
      const url = event.data.getRequestUrl();
      return native.shouldInterceptNavigation(url, args.webTag) as boolean;
    })
    .onPageEnd((event) => {
      // 主框架加载完成：Blazor 启动的确定性信号（.NET 侧以此触发 Blazor.start）
      native.notifyWebPageEnded(event.url, args.webTag);
      args.onPageEnded();
    })
}

@Entry
@Component
struct Index {
  private nodeContent: NodeContent = new NodeContent();
  // 固定 webTag：NDK ArkWeb API（SetJavaScriptProxyValidCallback / SetSchemeHandler）按 tag 寻址本组件
  private readonly webTag: string = 'harmony-web';
  private webController: webview.WebviewController = new webview.WebviewController(this.webTag);
  private externalBridge: ExternalBridge = new ExternalBridge();
  private nodeContentPassed: boolean = false;
  private webNode?: BuilderNode<[WebNodeArgs]>;
  private controllerReady: boolean = false;
  private pendingFrameNode: object | null = null;

  aboutToAppear(): void {
    native.initDotnet();

    // 内嵌模式：Web 由 BuilderNode 承载，节点交给 .NET 挂入 MAUI 布局容器。
    const node: BuilderNode<[WebNodeArgs]> = new BuilderNode<[WebNodeArgs]>(this.getUIContext());
    const args: WebNodeArgs = {
      controller: this.webController,
      bridge: this.externalBridge,
      webTag: this.webTag,
      onPageEnded: () => this.passNodeContentOnce(),
      onControllerReady: () => this.onControllerReady(),
    };
    node.build(wrapBuilder(WebNodeBuilder), args);
    this.webNode = node;

    const frameNode = node.getFrameNode();
    console.info(`[host] embedded web node=${frameNode === null ? 'null' : 'ok'}`);

    // 等 onControllerAttached（控制器与 Web 组件关联完成）再交给 .NET；
    // 提前调用 registerJavaScriptProxy 会抛 "must be associated with a Web component"。
    this.pendingFrameNode = frameNode;
    if (this.controllerReady) {
      this.passWebViewToDotNet();
    }
  }

  private onControllerReady(): void {
    this.controllerReady = true;
    this.passWebViewToDotNet();
  }

  private passWebViewToDotNet(): void {
    if (!this.controllerReady || this.pendingFrameNode === null) {
      return;
    }
    const node = this.pendingFrameNode;
    this.pendingFrameNode = null;
    native.passWebView(this.webController, this.webTag, node);
  }

  // NodeContent 只需交接一次（.NET 侧据此构建原生节点树）
  private passNodeContentOnce(): void {
    if (this.nodeContentPassed) {
      return;
    }
    this.nodeContentPassed = true;
    setTimeout(() => {
      native.passNodeContent(this.nodeContent);
    }, 0);
  }

  build() {
    Stack() {
      Column() {
        ContentSlot(this.nodeContent)
      }
    }
    .width('100%')
    .height('100%')
  }
}
