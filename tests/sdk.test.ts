import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { getSdkVersion, resolveSdkRoot, SdkRequirement } from '../tools/api-generator/sdk';

describe('HarmonyOS SDK resolution', () => {
    let tempRoot: string;

    beforeAll(() => {
        tempRoot = fs.mkdtempSync(path.join(os.tmpdir(), 'harmony-sdk-'));
    });

    afterAll(() => {
        fs.rmSync(tempRoot, { recursive: true, force: true });
    });

    function createSdk(name: string, requirement: SdkRequirement = 'full'): string {
        const root = path.join(tempRoot, name);
        fs.mkdirSync(path.join(root, 'ets', 'component'), { recursive: true });
        if (requirement !== 'native') {
            fs.mkdirSync(path.join(root, 'ets', 'api'), { recursive: true });
        }
        return root;
    }

    test('explicit SDK path validates the requested layout without fallback', () => {
        const nativeOnly = createSdk('native-only', 'native');

        expect(resolveSdkRoot('native', {
            explicitPath: nativeOnly,
            defaults: [path.join(tempRoot, 'full-sdk')],
        })).toBe(nativeOnly);
        expect(() => resolveSdkRoot('full', {
            explicitPath: nativeOnly,
            defaults: [],
        })).toThrow(/ets\/component \+ ets\/api/);
    });

    test('stale explicit environment candidate warns and falls through', () => {
        const fallback = createSdk('28.0.0');
        const warnings: string[] = [];
        const resolved = resolveSdkRoot('full', {
            env: {
                OHOS_SDK_BASE: path.join(tempRoot, 'missing-sdk'),
                OHOS_SDK_HOME: fallback,
            },
            defaults: [],
            onWarning: warning => warnings.push(warning),
        });

        expect(resolved).toBe(fallback);
        expect(warnings).toHaveLength(1);
        expect(warnings[0]).toContain('OHOS_SDK_BASE');
    });

    test('version-set root selects highest stable valid version deterministically', () => {
        const base = path.join(tempRoot, 'versions');
        createSdk(path.join('versions', '26.0.0'));
        createSdk(path.join('versions', '27.0.0'), 'native');
        createSdk(path.join('versions', '28.0.0'));
        createSdk(path.join('versions', '29.0.0-beta'));

        expect(resolveSdkRoot('full', { env: { OHOS_SDK_BASE: base }, defaults: [] }))
            .toBe(path.join(base, '28.0.0'));
    });

    test('nested DevEco sdk/default/openharmony layout is discovered', () => {
        const devecoSdk = path.join(tempRoot, 'deveco-sdk');
        const nested = path.join(devecoSdk, 'default', 'openharmony');
        fs.mkdirSync(path.join(nested, 'ets', 'component'), { recursive: true });
        fs.mkdirSync(path.join(nested, 'ets', 'api'), { recursive: true });

        expect(resolveSdkRoot('full', {
            env: { OHOS_SDK_BASE: devecoSdk },
            defaults: [],
        })).toBe(nested);
    });

    test('version metadata uses SDK version rather than machine path', () => {
        const root = path.join(tempRoot, 'metadata-sdk');
        fs.mkdirSync(root, { recursive: true });
        fs.writeFileSync(path.join(root, 'oh-uni-package.json'), JSON.stringify({ version: '27.0.1' }));

        expect(getSdkVersion(root)).toBe('27.0.1');
    });

    test('nested ets metadata platformVersion is preferred', () => {
        const root = path.join(tempRoot, 'nested-metadata-sdk');
        fs.mkdirSync(path.join(root, 'ets'), { recursive: true });
        fs.writeFileSync(
            path.join(root, 'ets', 'oh-uni-package.json'),
            JSON.stringify({ platformVersion: '28.1.2', version: 'internal-build' }),
        );
        fs.writeFileSync(path.join(root, 'package.json'), JSON.stringify({ version: '0.0.1' }));

        expect(getSdkVersion(root)).toBe('28.1.2');
    });

    test('no valid candidate reports all checked roots', () => {
        const missing = path.join(tempRoot, 'missing');
        expect(() => resolveSdkRoot('full', {
            env: { OHOS_SDK_BASE: missing },
            defaults: [],
            onWarning: jest.fn(),
        })).toThrow(new RegExp(`Checked: .*${missing.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`));
    });
});
