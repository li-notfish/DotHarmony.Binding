/**
 * @ohos.* 服务模块生成（apiGenerator + astParser 服务路径）的行为测试。
 * 覆盖 .NET 风格标准化的关键回归点：
 * - Promise<T> → Task<T>（含嵌套泛型 Promise<Array<T>>）
 * - on* 服务函数不再被误判为组件事件
 * - 嵌套接口 → JsObject 包装类 / 纯数据接口 → record
 * - 嵌套类构造函数（PhotoViewPicker 不再是空壳）
 */
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { ArkTsParser } from '../src/parser/index';
import { ApiGenerator } from '../src/parser/apiGenerator';

function writeTempDts(content: string): string {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-api-'));
    const file = path.join(dir, '@ohos.testsvc.d.ts');
    fs.writeFileSync(file, content);
    return file;
}

function generate(content: string): string {
    const file = writeTempDts(content);
    const parser = new ArkTsParser();
    const result = parser.parseFile(file);
    const gen = new ApiGenerator();
    const moduleInfo = ApiGenerator.dtsToModuleInfo(path.basename(file));
    return gen.generate(result.component, moduleInfo, [], result.enums, result.enums).csharp;
}

describe('@ohos.* 服务模块生成（.NET 风格）', () => {
    test('Promise<Array<Instance>> → Task<InstanceObject[]> 并带 Async 后缀', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Item { name: string; }
    function getAllItems(): Promise<Array<Item>>;
    function getItem(callback: AsyncCallback<Array<Item>>): void;
}
`);
        expect(cs).toContain('public static Task<Item[]> GetAllItemsAsync()');
        // AsyncCallback 命名形式重载折叠进同一 Task 方法
        expect(cs).not.toContain('Item[] GetAllItems()');
    });

    test('on* 服务函数保持普通方法（不误判为组件事件）', () => {
        const cs = generate(`
declare namespace testsvc {
    function onChangeWithAttribute(option: Array<string>, callback: Callback<number>): void;
    function on(type: string, callback: Callback<void>): void;
}
`);
        expect(cs).toContain('OnChangeWithAttribute(');
        expect(cs).toContain('On(');
    });

    test('有方法的嵌套接口生成 JsObject 派生包装类', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Paste {
        readonly recordCount: number;
        getRecord(index: number): string;
    }
    function getPaste(): Paste;
}
`);
        expect(cs).toContain('public sealed partial class Paste : JsObject');
        expect(cs).toContain('public double RecordCount =>');
        expect(cs).toContain('public string GetRecord(double index)');
        // 返回位置的接口 → 包装类
        expect(cs).toContain('public static Paste GetPaste()');
    });

    test('纯数据入参接口生成 record（INapiRecord）', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Rect {
        left: number;
        top: number;
    }
    function draw(rect: Rect): void;
}
`);
        expect(cs).toContain('public sealed record Rect(');
        expect(cs).toContain(') : INapiRecord');
        expect(cs).toContain('public static void Draw(Rect rect)');
    });

    test('返回位置引用的纯数据接口升级为包装类', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Size {
        width: number;
        height: number;
    }
    interface Provider {
        getSize(): Size;
    }
    function getProvider(): Provider;
}
`);
        expect(cs).toContain('public sealed partial class Size : JsObject');
        expect(cs).toContain('public Size GetSize()');
    });

    test('嵌套类带构造函数：PhotoViewPicker 不再是空壳', () => {
        const cs = generate(`
declare namespace testsvc {
    class PhotoViewPicker {
        constructor();
        select(): Promise<string>;
    }
}
`);
        expect(cs).toContain('public sealed partial class PhotoViewPicker : JsObject');
        expect(cs).toContain('NodeApi.CreateInstance(Testsvc.Module, _PhotoViewPicker)');
        expect(cs).toContain('Task<string> SelectAsync()');
    });

    test('Task 返回方法带 Async 后缀；同步方法不加', () => {
        const cs = generate(`
declare namespace testsvc {
    function load(): Promise<string>;
    function loadSync(): string;
}
`);
        expect(cs).toContain('public static Task<string> LoadAsync()');
        expect(cs).toContain('public static string LoadSync()');
    });

    test('成员名缩写词归一（getURI → GetUri）', () => {
        const cs = generate(`
declare namespace testsvc {
    function getURI(path: string): Promise<string>;
    const MIMETYPE_TEXT: string;
}
`);
        expect(cs).toContain('GetUriAsync(');
        expect(cs).toContain('public static string MimeTypeText =>');
    });
});

describe('AsyncCallback 双形态判定（callback-only → CallbackTaskBridge）', () => {
    test('仅 callback 形式（无 Promise 重载）→ CallMethodAsyncCallback 传入回调', () => {
        const cs = generate(`
declare namespace testsvc {
    function subscribe(callback: AsyncCallback<number>): void;
}
`);
        expect(cs).toContain('public static Task<double> SubscribeAsync()');
        expect(cs).toContain('NodeApi.CallMethodAsyncCallback<double>(Module, _subscribe, null)');
    });

    test('双形态（callback + Promise 重载）→ 仍走 Promise 通道并折叠为单一 Task 方法', () => {
        const cs = generate(`
declare namespace testsvc {
    function get(key: string, callback: AsyncCallback<string>): void;
    function get(key: string): Promise<string>;
}
`);
        expect(cs).toContain('NodeApi.CallMethodAsync<string>(Module, _get');
        expect(cs).not.toContain('CallMethodAsyncCallback');
        expect(cs.match(/Task<string> GetAsync\(/g)?.length).toBe(1);
    });

    test('AsyncCallback<void> 仅 callback 形式 → CallMethodAsyncCallbackVoid', () => {
        const cs = generate(`
declare namespace testsvc {
    function unsubscribe(callback?: AsyncCallback<void>): void;
}
`);
        expect(cs).toContain('public static Task UnsubscribeAsync()');
        expect(cs).toContain('NodeApi.CallMethodAsyncCallbackVoid(Module, _unsubscribe)');
    });

    test('实例方法仅 callback 形式 → 实例 CallMethodAsyncCallback 助手', () => {
        const cs = generate(`
declare namespace testsvc {
    interface File {
        read(len: number, callback: AsyncCallback<number>): void;
    }
    function open(): File;
}
`);
        expect(cs).toContain('public Task<double> ReadAsync(double len)');
        expect(cs).toContain('CallMethodAsyncCallback<double>(_read, null, len)');
    });
});
