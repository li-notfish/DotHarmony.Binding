import * as fs from 'fs';
import * as path from 'path';

describe('HarmonyOS sample NativeAOT exports', () => {
  const samplesRoot = path.resolve(__dirname, '..', 'samples', 'dotnet');
  const sampleNames = fs.readdirSync(samplesRoot).filter((name) =>
    fs.existsSync(path.join(samplesRoot, name, 'Program.cs')),
  );

  test('every sample provides a HarmonyInit export marked with EntryPoint', () => {
    expect(sampleNames.length).toBeGreaterThanOrEqual(6);

    for (const sampleName of sampleNames) {
      const exportPath = path.join(
        samplesRoot,
        sampleName,
        'Platforms',
        'HarmonyOS',
        'HarmonyExports.cs',
      );
      expect(fs.existsSync(exportPath)).toBe(true);

      const source = fs.readFileSync(exportPath, 'utf8');
      expect(source).toContain('UnmanagedCallersOnly');
      expect(source).toContain('EntryPoint');
      expect(source).toMatch(
        /\[UnmanagedCallersOnly\(EntryPoint\s*=\s*"HarmonyInit"\)\]/,
      );
      expect(source).toMatch(/ILC[\s\S]*UnmanagedCallersOnly/);

      const nativeExportsStart = source.indexOf('class NativeExports');
      const bootstrapStart = source.indexOf('class Bootstrap');
      expect(nativeExportsStart).toBeGreaterThanOrEqual(0);
      if (bootstrapStart >= 0) {
        expect(bootstrapStart).toBeGreaterThan(nativeExportsStart);
      }
      const nativeExports = source.slice(
        nativeExportsStart,
        bootstrapStart >= 0 ? bootstrapStart : source.length,
      );
      const attributeCount = (nativeExports.match(/\[UnmanagedCallersOnly\(/g) ?? []).length;
      const methodCount = (
        nativeExports.match(
          /\b(?:private|internal|public)\s+static\s+[A-Za-z_][\w.<>,\[\]?]*\s+[A-Za-z_]\w*\s*\(/g,
        ) ?? []
      ).length;
      expect(attributeCount).toBeGreaterThan(0);
      expect(attributeCount).toBe(methodCount);
      expect(nativeExports).not.toMatch(
        /\[UnmanagedCallersOnly\s*\]/,
      );
    }
  });
});
