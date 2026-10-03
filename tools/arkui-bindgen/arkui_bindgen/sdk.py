"""SDK / NDK 头文件目录探测（与历史 extract_arkui_types.py、scripts/check-abi-mirror.ps1 同序）。

优先级：--sdk 显式参数 > OHOS_SDK_BASE > OHSDK_HOME > DevEco 内置 SDK
（D:/Program Files/Huawei/DevEco Studio，现默认安装位置）> 本机历史默认
（D:/Harmony/OpenHarmony/Sdk，已迁出）。每个候选依次尝试 “版本号子目录 /
openharmony 子目录 / 根” 三种形态。
"""
from __future__ import annotations

import os
from pathlib import Path

DEFAULT_SDK_HOME = "D:/Harmony/OpenHarmony/Sdk"
DEVECO_SDKS = [
    "D:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony",
    "C:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony",
]
DEFAULT_VERSION = "26.0.0"

_NATIVE_INCLUDE = ("native", "sysroot", "usr", "include")


def _candidates(home: str) -> list[Path]:
    base = Path(home)
    native = base.joinpath(*_NATIVE_INCLUDE)
    return [
        base.joinpath(DEFAULT_VERSION, *_NATIVE_INCLUDE),
        base / "openharmony" / Path(*_NATIVE_INCLUDE),
        native,
    ]


def find_include_dir(sdk: str | None = None, probe: str = "arkui/native_node.h") -> Path:
    """返回包含 probe（默认 arkui/native_node.h）的 include 根目录。"""
    homes: list[Path] = []
    if sdk:
        homes.append(Path(sdk))
    for var in ("OHOS_SDK_BASE", "OHSDK_HOME"):
        v = os.getenv(var)
        if v:
            homes.append(Path(v))
    if not sdk:
        homes.extend(Path(d) for d in DEVECO_SDKS)
        homes.append(Path(DEFAULT_SDK_HOME))
    for home in homes:
        # 显式/环境候选：若直接指向 include 根也接受
        if (home / probe).is_file():
            return home
        for c in _candidates(str(home)):
            if (c / probe).is_file():
                return c
    tried = "; ".join(str(h) for h in homes) or "(空)"
    raise SystemExit(
        f"error: 未找到 NDK 头文件（{probe}）。用 --sdk 指定 SDK 根，或设置 OHOS_SDK_BASE/OHSDK_HOME。"
        f"（已尝试根目录: {tried}）"
    )
