// 异步服务测试用例：验证 Promise<T> → Task<T> 代码生成
// 覆盖场景：string/number/boolean 返回、void 返回、复杂类型退回 IntPtr、同步方法对照

export declare function load(url: string): Promise<string>;
export declare function getCount(): Promise<number>;
export declare function isEnabled(): Promise<boolean>;
export declare function doWork(): Promise<void>;
export declare function getHandle(): Promise<SomeComplexType>;
export declare function syncMethod(): number;
