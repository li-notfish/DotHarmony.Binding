/**
 * ArrayBuffer/BigInt/Map 封送与 .NET 事件模型的生成测试。
 */
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { TypeMapper } from '../src/parser/typeMapper';
import { ArkTsParser } from '../src/parser/index';
import { ApiGenerator } from '../src/parser/apiGenerator';

function generate(content: string, fileName = '@ohos.testsvc.d.ts'): string {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ohos-ev-'));
    const file = path.join(dir, fileName);
    fs.writeFileSync(file, content);
    const parser = new ArkTsParser();
    const result = parser.parseFile(file);
    const gen = new ApiGenerator();
    return gen.generate(result.component, ApiGenerator.dtsToModuleInfo(path.basename(file)), [], result.enums, result.enums).csharp;
}

describe('封送类型映射（TypeMapper）', () => {
    test('ArrayBuffer/TypedArray → byte[]', () => {
        expect(TypeMapper.mapType('ArrayBuffer')).toBe('byte[]');
        expect(TypeMapper.mapType('Uint8Array')).toBe('byte[]');
        expect(TypeMapper.mapType('Promise<ArrayBuffer>')).toBe('Task<byte[]>');
    });

    test('bigint → JsBigInt（不是裸 long，保证走 napi_create_bigint 通道）', () => {
        expect(TypeMapper.mapType('bigint')).toBe('JsBigInt');
        expect(TypeMapper.mapType('Promise<bigint>')).toBe('Task<JsBigInt>');
    });

    test('Map<K,V> → JsMap 活视图', () => {
        TypeMapper.addMapping('Geofence', 'GeofenceObject');
        expect(TypeMapper.mapType('Map<number, Geofence>')).toBe('JsMap<double, GeofenceObject>');
        expect(TypeMapper.mapType('Map<string, string>')).toBe('JsMap<string, string>');
        expect(TypeMapper.mapType('Promise<Map<number, Geofence>>')).toBe('Task<JsMap<double, GeofenceObject>>');
        TypeMapper.removeMapping('Geofence');
    });
});

describe('事件模型生成', () => {
    test('类型化 On/Off + 字面量 .NET event（add/remove 经 EventListenerRegistry 配对）', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Payload { level: number; }
    function on(type: 'levelChange', callback: Callback<Payload>): void;
    function off(type: 'levelChange', callback?: Callback<Payload>): void;
}
`);
        // 类型化重载（回调参数不再是 IntPtr）
        expect(cs).toContain('public static void On(string type, System.Action<Payload> callback)');
        expect(cs).toContain('public static void Off(string type, System.Action<Payload> callback)');
        expect(cs).toContain('public static void Off(string type)');
        // .NET event 访问器
        expect(cs).toContain('public static event System.Action<Payload> LevelChange');
        expect(cs).toContain('_eventListeners.Add(("levelChange", value)');
        expect(cs).toContain('_eventListeners.Remove(("levelChange", value)');
        // 注册表 + 共享跳板链路
        expect(cs).toContain('private static readonly EventListenerRegistry _eventListeners = new();');
    });

    test('回调载荷新封送类型：ArrayBuffer → byte[]，事件适配器自动转换', () => {
        const cs = generate(`
declare namespace testsvc {
    function on(type: 'dataReceive', callback: Callback<ArrayBuffer>): void;
    function off(type: 'dataReceive', callback?: Callback<ArrayBuffer>): void;
}
`);
        expect(cs).toContain('System.Action<byte[]> callback');
        expect(cs).toContain('NativeValue.ToByteArray(args[0])');
    });

    test('on 但没有 off 的模块：事件访问器可生成（remove 走 _off，u8 常量自动补齐）', () => {
        const cs = generate(`
declare namespace testsvc {
    function on(type: 'available', callback: Callback<void>): void;
}
`);
        expect(cs).toContain('private static ReadOnlySpan<byte> _off => "off"u8;');
        expect(cs).toContain('public static event System.Action Available');
        // d.ts 没有 off 函数 → 不生成 Off 方法
        expect(cs).not.toContain('public static void Off(string type)');
    });

    test('事件名与既有方法撞名时追加 Event 后缀', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Player {
        pause(): void;
        on(type: 'pause', callback: Callback<void>): void;
        off(type: 'pause', callback?: Callback<void>): void;
    }
    function getPlayer(): Player;
}
`);
        expect(cs).toContain('public event System.Action PauseEvent');
        expect(cs).not.toContain('public event System.Action Pause\n');
    });

    test('实例类型上的事件（包装类实例事件）', () => {
        const cs = generate(`
declare namespace testsvc {
    interface Conn {
        on(type: 'netLost', callback: Callback<number>): void;
        off(type: 'netLost', callback?: Callback<number>): void;
    }
    function createConn(): Conn;
}
`);
        expect(cs).toContain('public event System.Action<double> NetLost');
        expect(cs).toContain('private readonly EventListenerRegistry _eventListeners = new();');
    });
});
