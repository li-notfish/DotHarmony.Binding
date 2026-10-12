/**
 * C shim 导出的 ArkTS 接口
 */
export const initDotnet: () => void;
export const passNodeContent: (content: NodeContent) => void;
export const popPage: () => boolean;
export const notifyThemeChanged: (colorMode: number) => void;
export const notifyLifecycleChanged: (event: number) => void;
export const initEngine: (bridge: Object) => void;
