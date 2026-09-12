import { toPascalCase, withAsyncSuffix } from '../src/parser/naming';

describe('naming（.NET 风格命名规范化）', () => {
    describe('toPascalCase', () => {
        test('camelCase 方法名', () => {
            expect(toPascalCase('getDefaultDisplaySync')).toBe('GetDefaultDisplaySync');
            expect(toPascalCase('vibrate')).toBe('Vibrate');
            expect(toPascalCase('sdkApiVersion')).toBe('SdkApiVersion');
        });

        test('缩写词归一（三字母以上首字母大写）', () => {
            expect(toPascalCase('getURI')).toBe('GetUri');
            expect(toPascalCase('getURL')).toBe('GetUrl');
            expect(toPascalCase('setHTTPProxy')).toBe('SetHttpProxy');
            expect(toPascalCase('getDeviceId')).toBe('GetDeviceId');
            expect(toPascalCase('queryDnsAscii')).toBe('QueryDnsAscii');
            expect(toPascalCase('getUDID')).toBe('GetUdid');
            expect(toPascalCase('abiList')).toBe('AbiList');
        });

        test('两字母缩写保留全大写', () => {
            expect(toPascalCase('getIP')).toBe('GetIP');
            expect(toPascalCase('getTVPowerMode')).toBe('GetTVPowerMode');
        });

        test('SCREAMING_SNAKE 枚举成员切词', () => {
            expect(toPascalCase('TYPE_DEFAULT')).toBe('TypeDefault');
            expect(toPascalCase('MIMETYPE_TEXT_PLAIN')).toBe('MimeTypeTextPlain');
            expect(toPascalCase('ROUNDED_SQUARE')).toBe('RoundedSquare');
            expect(toPascalCase('TYPE_2IN1')).toBe('Type2In1');
        });

        test('单词保持', () => {
            expect(toPascalCase('name')).toBe('Name');
            expect(toPascalCase('INVERT')).toBe('Invert');
        });
    });

    describe('withAsyncSuffix', () => {
        test('Task 返回加 Async 后缀', () => {
            expect(withAsyncSuffix('GetDefaultDisplay', 'Task<DisplayObject>')).toBe('GetDefaultDisplayAsync');
            expect(withAsyncSuffix('StartVibration', 'Task')).toBe('StartVibrationAsync');
        });

        test('同步返回不加后缀', () => {
            expect(withAsyncSuffix('GetDefaultDisplaySync', 'DisplayObject')).toBe('GetDefaultDisplaySync');
            expect(withAsyncSuffix('IsFoldable', 'bool')).toBe('IsFoldable');
        });

        test('已带 Async 不重复', () => {
            expect(withAsyncSuffix('GetAllDisplayAsync', 'Task')).toBe('GetAllDisplayAsync');
        });
    });
});
