import { CliActions, runCli } from '../tools/api-generator/index';

describe('api-generator CLI', () => {
    const originalExitCode = process.exitCode;
    let errorSpy: jest.SpyInstance;

    beforeEach(() => {
        process.exitCode = undefined;
        errorSpy = jest.spyOn(console, 'error').mockImplementation();
    });

    afterEach(() => {
        errorSpy.mockRestore();
        process.exitCode = originalExitCode;
    });

    function actions(overrides: Partial<CliActions> = {}): CliActions {
        return {
            native: async () => undefined,
            full: async () => undefined,
            default: async () => undefined,
            ...overrides,
        };
    }

    test('native mode passes explicit SDK path to native generation', async () => {
        let received: string | undefined;
        const code = await runCli(
            ['--native', '--sdk', 'D:/sdk/27.0.0'],
            actions({ native: async sdk => { received = sdk; } }),
        );

        expect(code).toBe(0);
        expect(received).toBe('D:/sdk/27.0.0');
        expect(process.exitCode).toBeUndefined();
    });

    test('sdk flag invokes full generation and preserves all-modules flag', async () => {
        let received: { sdk?: string; all: boolean } | undefined;
        const code = await runCli(
            ['--sdk', 'D:/sdk/28.0.0', '--all'],
            actions({ full: async (sdk, all) => { received = { sdk, all }; } }),
        );

        expect(code).toBe(0);
        expect(received).toEqual({ sdk: 'D:/sdk/28.0.0', all: true });
    });

    test('without generator flags invokes default generation', async () => {
        let invoked = false;
        const code = await runCli([], actions({ default: async () => { invoked = true; } }));

        expect(code).toBe(0);
        expect(invoked).toBe(true);
    });

    test('generation failure prints once and sets exit code one', async () => {
        const code = await runCli(
            ['--native'],
            actions({
                native: async () => {
                    throw new Error('native generation failed');
                },
            }),
        );

        expect(code).toBe(1);
        expect(process.exitCode).toBe(1);
        expect(errorSpy).toHaveBeenCalledTimes(1);
        expect(errorSpy.mock.calls[0][0]).toContain('native generation failed');
    });
});
