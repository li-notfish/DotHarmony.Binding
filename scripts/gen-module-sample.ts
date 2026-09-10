import * as fs from 'fs';
import { ArkTsParser } from '../src/parser/index';
import { ParseResult } from '../src/parser/models';

const p = new ArkTsParser();
// 合成 ParseResult：验证 void / Promise<string> / Promise<number> 的产出
const result = {
    component: {
        name: 'TestModule',
        interfaceName: 'TestModuleInterface',
        attributeName: 'TestModuleAttribute',
        constructorParams: [],
        constructorOverloads: [],
        methods: [
            { name: 'start', returnType: 'void', parameters: [], isChained: false },
            { name: 'load', returnType: 'Promise<string>', parameters: [{ name: 'url', type: 'string' }], isChained: false },
            { name: 'getCount', returnType: 'Promise<number>', parameters: [], isChained: false },
            { name: 'read', returnType: 'Promise<string>', parameters: [{ name: 'path', type: 'string' }], isChained: false },
        ],
        events: [],
        delegates: [],
        namespace: 'HarmonyOS.Api.Test',
    },
    imports: [],
    warnings: [],
} as unknown as ParseResult;

const cs = p.generateCode(result);
fs.writeFileSync('tmp/testmodule.gen.cs', cs);
console.log('written', cs.length, 'chars');
