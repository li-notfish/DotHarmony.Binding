import { webview } from '@kit.ArkWeb';
import { NodeContent, BuilderNode } from '@kit.ArkUI';
import native from 'libentry.so';

class ExternalBridge {
  private receiveMessageCallback?: (message: string) => void;
  // 多实例：每条消息带 webTag，.NET 侧据此路由到对应 surface。
  private readonly webTag: string;

  constructor(webTag: string) {
    this.webTag = webTag;
  }

  receiveMessage(callback: (message: string) => void): void {
    this.receiveMessageCallback = callback;
  }

  sendMessage(message: string): void {
    native.receiveWebViewMessage(message, this.webTag);
  }

  dispatchMessage(message: string): void {
    if (this.receiveMessageCallback) {
      this.receiveMessageCallback(message);
    }
  }
}

interface WebNodeArgs {
  controller: webview.WebviewController;
  bridge: ExternalBridge;
  webTag: string;
  onPageEnded: () => void;
  onControllerReady: () => void;
}

/**
 * Web 的 BuilderNode 版本（内嵌模式）：Web 不由页面 build() 直接声明，
 * 而是包成节点交给 .NET，由 MAUI 布局决定尺寸与位置。
 * 所有 Web 级回调（proxy / 导航拦截 / 页面事件）都挂在 Builder 内的 Web 上。
 */
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

/** 单个 Web 实例的宿主侧状态。 */
class WebSlot {
  readonly tag: string;
  readonly controller: webview.WebviewController;
  readonly bridge: ExternalBridge;
  node?: BuilderNode<[WebNodeArgs]>;
  frameNode: object | null = null;
  controllerReady: boolean = false;
  passed: boolean = false;

  constructor(tag: string) {
    this.tag = tag;
    this.controller = new webview.WebviewController(tag);
    this.bridge = new ExternalBridge(tag);
  }
}

@Entry
@Component
struct Index {
  private nodeContent: NodeContent = new NodeContent();
  private nodeContentPassed: boolean = false;
  // 多实例：每个 slot 一个 webTag → ArkWeb 按 tag 寻址；.NET 侧按 tag 注册独立 surface。
  // 默认实例用 'harmony-web'，保持与单实例时代的兼容；其余由 .NET 侧按需声明。
  private readonly slots: WebSlot[] = [new WebSlot('harmony-web')];

  aboutToAppear(): void {
    native.initDotnet();

    for (const slot of this.slots) {
      this.mountSlot(slot);
    }

    // .NET 侧可在任意 BlazorWebView 连接时按需请求新实例（WebTag 属性）
    native.registerWebViewFactory((tag: string) => {
      this.ensureSlot(tag);
    });
  }

  /** 为一个 webTag 创建 BuilderNode 并把节点交给 .NET（幂等）。 */
  private mountSlot(slot: WebSlot): void {
    if (slot.node !== undefined) {
      return;
    }
    const node: BuilderNode<[WebNodeArgs]> = new BuilderNode<[WebNodeArgs]>(this.getUIContext());
    const args: WebNodeArgs = {
      controller: slot.controller,
      bridge: slot.bridge,
      webTag: slot.tag,
      onPageEnded: () => this.passNodeContentOnce(),
      onControllerReady: () => this.onControllerReady(slot),
    };
    node.build(wrapBuilder(WebNodeBuilder), args);
    slot.node = node;
    slot.frameNode = node.getFrameNode();
    console.info(`[host] web slot '${slot.tag}' node=${slot.frameNode === null ? 'null' : 'ok'}`);
  }

  /** 按 tag 查找或新增 slot 并挂载（供 .NET 侧按需请求）。 */
  private ensureSlot(tag: string): void {
    if (this.slots.some((s: WebSlot) => s.tag === tag)) {
      return;
    }
    const slot = new WebSlot(tag);
    this.slots.push(slot);
    this.mountSlot(slot);
  }

  private onControllerReady(slot: WebSlot): void {
    slot.controllerReady = true;
    this.passWebViewToDotNet(slot);
  }

  /**
   * 把某个 slot 的 controller + webTag + FrameNode 交给 .NET。
   * 必须等 onControllerAttached：提前调用 registerJavaScriptProxy 会抛
   * "must be associated with a Web component"。
   */
  private passWebViewToDotNet(slot: WebSlot): void {
    if (slot.passed || !slot.controllerReady || slot.frameNode === null) {
      return;
    }
    slot.passed = true;
    native.passWebView(slot.controller, slot.tag, slot.frameNode);
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
