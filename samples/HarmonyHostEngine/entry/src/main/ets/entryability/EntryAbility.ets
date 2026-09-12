import { AbilityConstant, UIAbility, Want } from '@kit.AbilityKit';
import { hilog } from '@kit.PerformanceAnalysisKit';
import { window } from '@kit.ArkUI';

const TAG: string = 'HarmonyHost';

export default class EntryAbility extends UIAbility {
  onCreate(want: Want, launchParam: AbilityConstant.LaunchParam): void {
    hilog.info(0x0000, TAG, 'Ability onCreate');
    // .NET 侧 Promise 桥测试用的全局函数：
    // somePromiseApi → resolve(number)；willFail → reject(string reason)
    globalThis.somePromiseApi = (): Promise<number> => {
      return new Promise<number>((resolve) => { resolve(42); });
    };
    globalThis.willFail = (): Promise<string> => {
      return new Promise<string>((_, reject) => { reject('intentional failure for test'); });
    };
    // 封送专项测试：ArrayBuffer 原样返回（校验 byte[] → ArrayBuffer → byte[] 内容一致）；
    // makeMap → Map<number, string>（校验 JsMap 活视图迭代/查询）；sumMap → 遍历 Map 求和
    //（校验 JsMap 写入侧经 JsObject 引用传递）。
    globalThis.echoArrayBuffer = (ab: ArrayBuffer): ArrayBuffer => { return ab; };
    globalThis.makeMap = (): Map<number, string> => {
      const m = new Map<number, string>();
      m.set(1, 'one');
      m.set(2, 'two');
      m.set(3, 'three');
      return m;
    };
    globalThis.sumMap = (m: Map<number, number>): number => {
      let sum = 0;
      m.forEach((v) => { sum += v; });
      return sum;
    };
  }

  onDestroy(): void {
    hilog.info(0x0000, TAG, 'Ability onDestroy');
  }

  onWindowStageCreate(windowStage: window.WindowStage): void {
    hilog.info(0x0000, TAG, 'Ability onWindowStageCreate');
    windowStage.loadContent('pages/Index', (err) => {
      if (err.code) {
        hilog.error(0x0000, TAG, 'loadContent failed: %{public}s', JSON.stringify(err));
        return;
      }
      hilog.info(0x0000, TAG, 'loadContent succeeded');
    });
  }

  onWindowStageDestroy(): void {
    hilog.info(0x0000, TAG, 'Ability onWindowStageDestroy');
  }

  onForeground(): void {
    hilog.info(0x0000, TAG, 'Ability onForeground');
  }

  onBackground(): void {
    hilog.info(0x0000, TAG, 'Ability onBackground');
  }
}
