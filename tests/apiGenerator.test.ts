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

test('事件回调强制必需后，其前的可选参数须降级（CS1737）', () => {
    const cs = generate(`
declare namespace testsvc {
    interface SensorInfoParamX { a?: number; }
    function off(type: string, sensorInfoParam?: SensorInfoParamX, callback?: Callback<SensorInfoParamX>): void;
}
`);
    expect(cs).not.toMatch(/SensorInfoParamX\? \w+ = null, IntPtr callback/);
});

describe('跨模块强类型解析（M2 残留：导入类型不再无条件 IntPtr）', () => {
    function generateTwo(files: Record<string, string>): { owner: string; importer: string } {
        const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-xmod-'));
        const parser = new ArkTsParser();
        const results: Record<string, string> = {};
        const parsed: Record<string, { component: any; moduleInfo: any; imports: any[] }> = {};
        // 预处理阶段同序：先解析全部，再按序认领类型名
        for (const name of Object.keys(files)) {
            const file = path.join(dir, name);
            fs.writeFileSync(file, files[name]);
            parsed[name] = { ...parser.parseFile(file), moduleInfo: ApiGenerator.dtsToModuleInfo(name) };
        }
        for (const name of Object.keys(files)) {
            const p = parsed[name];
            for (const iface of p.component.interfaces) {
                ApiGenerator.claimTypeName(iface.name, p.moduleInfo.className);
            }
        }
        for (const name of Object.keys(files)) {
            const p = parsed[name];
            results[name] = new ApiGenerator().generate(
                p.component, p.moduleInfo, [], [], [],
                { imports: new Map(), demandedTs: new Set(), returnDemandTs: new Set() }
            ).csharp;
        }
        return { owner: results['@ohos.owner.d.ts'], importer: results['@ohos.importer.d.ts'] };
    }

    test('返回位跨模块引用 → 完全限定包装类型 + 工厂（来源模块转正）', () => {
        const { importer } = generateTwo({
            '@ohos.owner.d.ts': `
declare namespace owner {
    interface DeviceInfoRec { name: string; mac: string; }
}
`,
            '@ohos.importer.d.ts': `
import type { DeviceInfoRec } from './@ohos.owner';
declare namespace importer {
    function getDevice(): Promise<DeviceInfoRec>;
    function setDevice(device: DeviceInfoRec): void;
}
`,
        });
        // generateTwo 传入空解析表（来源模块未转正/灰度等价场景）→ 跨模块引用保守降级 IntPtr
        expect(importer).toContain('public static Task<IntPtr> GetDeviceAsync()');
        expect(importer).toContain('public static void SetDevice(IntPtr device)');
        // importer 侧手工传入解析表（模拟 index.ts 预计算：来源模块转正 → FQN）
        const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-xmod2-'));
        const parser = new ArkTsParser();
        const file = path.join(dir, '@ohos.importer2.d.ts');
        fs.writeFileSync(file, `
import type { DeviceInfoRec } from './@ohos.owner';
declare namespace importer2 {
    function getDevice(): Promise<DeviceInfoRec>;
}
`);
        const parsed = parser.parseFile(file);
        const moduleInfo = ApiGenerator.dtsToModuleInfo(path.basename(file));
        const imports = new Map([['DeviceInfoRec', 'global::HarmonyOS.Bindings.Api.DeviceInfoRec']]);
        const cs = new ApiGenerator().generate(
            parsed.component, moduleInfo, [], [], [],
            { imports, demandedTs: new Set(), returnDemandTs: new Set(['DeviceInfoRec']) }
        ).csharp;
        expect(cs).toContain('public static Task<global::HarmonyOS.Bindings.Api.DeviceInfoRec> GetDeviceAsync()');
        expect(cs).toContain('static h => new global::HarmonyOS.Bindings.Api.DeviceInfoRec(h)');
        // IntPtr 解析表（来源模块灰度等价场景）→ 保守降级
        const cs2 = new ApiGenerator().generate(
            parsed.component, moduleInfo, [], [], [],
            { imports: new Map([['DeviceInfoRec', 'IntPtr']]), demandedTs: new Set(), returnDemandTs: new Set() }
        ).csharp;
        expect(cs2).toContain('public static Task<IntPtr> GetDeviceAsync()');
    });

    test('返回位 demand 强制 owner 发射本模块无成员引用的类型（record 升级 wrapper）', () => {
        // owner 定义 Config 但模块自身无任何成员引用；importer 在返回位引用
        // → owner 必须发射（否则引用方 CS0246）
        const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-xmod3-'));
        const parser = new ArkTsParser();
        const ownerFile = path.join(dir, '@ohos.owner3.d.ts');
        fs.writeFileSync(ownerFile, `
declare namespace owner3 {
    interface Config { name: string; }
}
`);
        const parsed = parser.parseFile(ownerFile);
        const moduleInfo = ApiGenerator.dtsToModuleInfo(path.basename(ownerFile));
        ApiGenerator.claimTypeName('Config', moduleInfo.className);
        const cs = new ApiGenerator().generate(
            parsed.component, moduleInfo, [], [], [],
            { imports: new Map(), demandedTs: new Set(['Config']), returnDemandTs: new Set(['Config']) }
        ).csharp;
        expect(cs).toContain('class Config : JsObject');
    });

    test('输入位 demand 钉住不可封送 record（收敛不移除，引用方不 CS0246）', () => {
        const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-xmod4-'));
        const parser = new ArkTsParser();
        const ownerFile = path.join(dir, '@ohos.owner4.d.ts');
        fs.writeFileSync(ownerFile, `
declare namespace owner4 {
    interface Options { name: string; extra: UnknownThing; }
}
`);
        const parsed = parser.parseFile(ownerFile);
        const moduleInfo = ApiGenerator.dtsToModuleInfo(path.basename(ownerFile));
        ApiGenerator.claimTypeName('Options', moduleInfo.className);
        // demandedTs（输入位）→ record 钉住：属性含未注册类型（IntPtr）通常被收敛移除，钉住后仍发射
        const cs = new ApiGenerator().generate(
            parsed.component, moduleInfo, [], [], [],
            { imports: new Map(), demandedTs: new Set(['Options']), returnDemandTs: new Set() }
        ).csharp;
        expect(cs).toContain('record Options');
        expect(cs).toContain('IntPtr Extra');
    });
});
