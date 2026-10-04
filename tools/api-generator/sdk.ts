import * as fs from 'fs';
import * as path from 'path';

export type SdkRequirement = 'native' | 'full';

export interface ResolveSdkOptions {
    explicitPath?: string;
    env?: NodeJS.ProcessEnv;
    defaults?: string[];
    onWarning?: (message: string) => void;
}

export const DEFAULT_SDK_CANDIDATES = [
    'C:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony',
    'D:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony',
    'D:\\Harmony\\OpenHarmony\\Sdk',
    'C:\\Harmony\\OpenHarmony\\Sdk',
];

function hasLayout(root: string, requirement: SdkRequirement): boolean {
    if (!fs.existsSync(path.join(root, 'ets', 'component'))) return false;
    return requirement === 'native' || fs.existsSync(path.join(root, 'ets', 'api'));
}

function requiredLayoutLabel(requirement: SdkRequirement): string {
    return requirement === 'native' ? 'ets/component' : 'ets/component + ets/api';
}

function compareVersionNames(left: string, right: string): number {
    const parse = (name: string): { parts: number[]; prerelease: string } | null => {
        const match = name.match(/^(\d+(?:\.\d+)*)(?:-([0-9A-Za-z.-]+))?$/);
        if (!match) return null;
        return {
            parts: match[1].split('.').map(Number),
            prerelease: match[2] ?? '',
        };
    };
    const a = parse(left);
    const b = parse(right);
    if (!a && !b) return left.localeCompare(right);
    if (!a) return 1;
    if (!b) return -1;
    const length = Math.max(a.parts.length, b.parts.length);
    for (let i = 0; i < length; i++) {
        const delta = (a.parts[i] ?? 0) - (b.parts[i] ?? 0);
        if (delta !== 0) return delta;
    }
    if (a.prerelease === b.prerelease) return 0;
    if (!a.prerelease) return 1;
    if (!b.prerelease) return -1;
    return a.prerelease.localeCompare(b.prerelease);
}

function compareSdkCandidates(left: string, right: string): number {
    const versionPattern = /^\d+(?:\.\d+)*(?:-[0-9A-Za-z.-]+)?$/;
    const leftIsVersion = versionPattern.test(left);
    const rightIsVersion = versionPattern.test(right);
    if (!leftIsVersion && !rightIsVersion) return left.localeCompare(right);
    if (!leftIsVersion) return 1;
    if (!rightIsVersion) return -1;

    const leftPrerelease = left.includes('-');
    const rightPrerelease = right.includes('-');
    if (leftPrerelease !== rightPrerelease) return leftPrerelease ? 1 : -1;
    return compareVersionNames(right, left);
}

function childDirectories(root: string): string[] {
    if (!fs.existsSync(root)) return [];
    try {
        return fs.readdirSync(root, { withFileTypes: true })
            .filter(entry => entry.isDirectory())
            .map(entry => entry.name)
            .sort(compareSdkCandidates);
    } catch {
        return [];
    }
}

function discoverCandidateRoots(candidate: string, requirement: SdkRequirement, attempts: string[]): string[] {
    if (hasLayout(candidate, requirement)) return [candidate];
    if (!fs.existsSync(candidate)) {
        attempts.push(candidate);
        return [];
    }

    attempts.push(candidate);
    const found: string[] = [];
    const visit = (root: string, depth: number): void => {
        for (const name of childDirectories(root)) {
            const child = path.join(root, name);
            if (hasLayout(child, requirement)) {
                found.push(child);
            } else if (depth < 2) {
                visit(child, depth + 1);
            } else {
                attempts.push(child);
            }
        }
    };
    visit(candidate, 0);
    return found.sort((a, b) => compareSdkCandidates(path.basename(a), path.basename(b)));
}

export function resolveSdkRoot(
    requirement: SdkRequirement,
    options: ResolveSdkOptions = {},
): string {
    const warn = options.onWarning ?? ((message: string) => console.warn(message));
    const attempts: string[] = [];

    if (options.explicitPath) {
        if (hasLayout(options.explicitPath, requirement)) return options.explicitPath;
        throw new Error(
            `Explicit SDK path does not contain ${requiredLayoutLabel(requirement)}: ${options.explicitPath}`,
        );
    }

    const env = options.env ?? process.env;
    const envCandidates = [
        ['OHOS_SDK_BASE', env.OHOS_SDK_BASE],
        ['OHOS_SDK_HOME', env.OHOS_SDK_HOME],
        ['OHSDK_HOME', env.OHSDK_HOME],
    ] as const;

    for (const [name, candidate] of envCandidates) {
        if (!candidate) continue;
        if (hasLayout(candidate, requirement)) return candidate;
        const roots = discoverCandidateRoots(candidate, requirement, attempts);
        if (roots.length > 0) return roots[0];
        warn(`${name} does not contain a usable HarmonyOS SDK (${requiredLayoutLabel(requirement)}): ${candidate}`);
    }

    for (const candidate of options.defaults ?? DEFAULT_SDK_CANDIDATES) {
        if (hasLayout(candidate, requirement)) return candidate;
        const roots = discoverCandidateRoots(candidate, requirement, attempts);
        if (roots.length > 0) return roots[0];
        attempts.push(candidate);
    }

    const checked = [...new Set(attempts)].join(', ') || '(none)';
    throw new Error(
        `HarmonyOS SDK not found for layout ${requiredLayoutLabel(requirement)}. ` +
        `Pass --sdk <path> or set OHOS_SDK_BASE/OHSDK_HOME. Checked: ${checked}`,
    );
}

export function getSdkVersion(sdkRoot: string): string {
    const direct = path.basename(sdkRoot);
    if (/^\d+(?:\.\d+)*$/.test(direct)) return direct;

    for (const file of ['oh-uni-package.json', 'package.json']) {
        const candidate = path.join(sdkRoot, file);
        if (!fs.existsSync(candidate)) continue;
        try {
            const value = JSON.parse(fs.readFileSync(candidate, 'utf8')) as { version?: unknown };
            if (typeof value.version === 'string' && value.version) return value.version;
        } catch {
            // Ignore malformed metadata and continue with the next source.
        }
    }
    return 'unknown';
}
