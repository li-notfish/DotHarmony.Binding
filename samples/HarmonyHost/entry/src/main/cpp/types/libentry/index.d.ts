/**
 * C shim 导出的 ArkTS 接口
 */
export const initDotnet: () => void;
export const passNodeContent: (content: NodeContent) => void;
export const passWebView: (controller: object) => void;
export const interceptWebViewRequest: (
  url: string,
  createResponse: (code: number, message: string, mimeType: string, data: ArrayBuffer) => WebResourceResponse
) => WebResourceResponse;
export const receiveWebViewMessage: (message: string) => void;
