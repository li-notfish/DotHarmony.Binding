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
/** ArkTS 引擎桥通道（EngineLab；非引擎应用无需调用，dlsym 缺失时底层静默短路） */
export const initEngine: (bridge: object) => void;
