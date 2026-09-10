// AsyncCallback 服务测试用例：验证 (result: T, err?: Error) => void → Task<T> 代码生成

export declare class AsyncCallbackService {
    // 基础 AsyncCallback：string 结果
    getData(callback: (result: string, err?: Error) => void): void;
    
    // AsyncCallback：number 结果
    getCount(callback: (result: number, err?: Error) => void): void;
    
    // AsyncCallback：boolean 结果
    isEnabled(callback: (result: boolean, err?: Error) => void): void;
    
    // AsyncCallback：void 结果（仅错误回调）
    doWork(callback: (err?: Error) => void): void;
    
    // 混合参数：先有普通参数，后有 AsyncCallback
    fetchData(url: string, callback: (result: string, err?: Error) => void): void;
    
    // 同步方法（无 AsyncCallback）
    syncMethod(): number;
}
