import { webview } from '@kit.ArkWeb';
import { NodeContent } from '@kit.ArkUI';
import native from 'libentry.so';

function makeResponse(data: Resource, mimeType: string): WebResourceResponse {
  const response = new WebResourceResponse();
  response.setResponseData(data);
  response.setResponseMimeType(mimeType);
  response.setResponseEncoding('utf-8');
  response.setResponseCode(200);
  response.setReasonMessage('OK');
  response.setResponseIsReady(true);
  return response;
}

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
  private webController: webview.WebviewController = new webview.WebviewController();
  private externalBridge: ExternalBridge = new ExternalBridge();
  private readonly baseUri: string = 'https://localhost/';

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
        .onInterceptRequest((event) => {
          const url = event.request.getRequestUrl();
          const path = url.replace(this.baseUri, '');
          const response: WebResourceResponse | null = native.interceptWebViewRequest(url, (code: number, message: string, mimeType: string, data: ArrayBuffer): WebResourceResponse => {
            const response = new WebResourceResponse();
            response.setResponseData(data);
            response.setResponseCode(code);
            response.setReasonMessage(message);
            response.setResponseMimeType(mimeType);
            response.setResponseEncoding('utf-8');
            response.setResponseHeader([
              { headerKey: 'Access-Control-Allow-Origin', headerValue: 'null' },
              { headerKey: 'Access-Control-Allow-Credentials', headerValue: 'true' }
            ]);
            response.setResponseIsReady(true);
            return response;
          });
          if (response) {
            return response;
          }

          if (path === 'index.html' || path === '/' || path === '') {
            return makeResponse($rawfile('wwwroot/index.html'), 'text/html');
          }
          if (path === '_framework/blazor.webview.js') {
            return makeResponse($rawfile('wwwroot/_framework/blazor.webview.js'), 'text/javascript');
          }
          if (path === '_framework/blazor.modules.json') {
            return makeResponse($rawfile('wwwroot/_framework/blazor.modules.json'), 'application/json');
          }
          return null;
        })
        .onErrorReceive((event) => {
          console.error(`[host] error receive: ${event.request.getRequestUrl()}`);
        })
        .onPageEnd(() => {
          setTimeout(() => {
            native.passNodeContent(this.nodeContent);
          }, 0);
        })
    }
    .width('100%')
    .height('100%')
  }
}
