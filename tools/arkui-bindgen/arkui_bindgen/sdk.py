"""SDK / NDK 头文件目录探测（与历史 extract_arkui_types.py、scripts/check-abi-mirror.ps1 同序）。

优先级：--sdk 显式参数 > OHOS_SDK_BASE > OHSDK_HOME > DevEco 内置 SDK
（D:/Program Files/Huawei/DevEco Studio，现默认安装位置）> 本机历史默认
（D:/Harmony/OpenHarmony/Sdk，已迁出）。每个候选依次尝试 “版本号子目录 /
openharmony 子目录 / 根” 三种形态。
"""
from __future__ import annotations

import os
import json
import warnings
from pathlib import Path

DEFAULT_SDK_HOME = "D:/Harmony/OpenHarmony/Sdk"
DEVECO_SDKS = [
    "D:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony",
    "C:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony",
    "D:/Program Files/Huawei/DevEco Studio/sdk",
    "C:/Program Files/Huawei/DevEco Studio/sdk",
]

_NATIVE_INCLUDE = ("native", "sysroot", "usr", "include")


def _package_version(root: Path) -> str:
    for relative in (
        ("ets", "oh-uni-package.json"),
        ("oh-uni-package.json",),
        ("toolchains", "oh-uni-package.json"),
        ("package.json",),
    ):
        candidate = root.joinpath(*relative)
        try:
            data = json.loads(candidate.read_text(encoding="utf-8"))
        except (OSError, ValueError):
            continue
        for key in ("platformVersion", "version"):
            value = data.get(key)
            if isinstance(value, str) and value:
                return value
    return root.name


def _version_sort_key(root: Path) -> tuple[int, tuple[int, ...], str]:
    version = _package_version(root)
    stable = version.replace("-", ".", 1) == version
    parts = version.split("-", 1)[0].split(".")
    if not stable:
        rank = 1
    elif all(part.isdigit() for part in parts):
        rank = 0
    else:
        rank = 2
    numeric = tuple(int(part) for part in parts if part.isdigit()) if rank < 2 else ()
    return rank, tuple(-part for part in numeric), version


def _probe_root(root: Path, probe: str) -> tuple[Path, Path] | None:
    probe_parts = probe.split("/")
    direct = root.joinpath(*probe_parts)
    if direct.is_file():
        return root, root

    include_root = root.joinpath(*_NATIVE_INCLUDE)
    native_probe = include_root.joinpath(*probe_parts)
    if native_probe.is_file():
        return root, include_root
    return None


def _discover_roots(home: Path, probe: str) -> list[Path]:
    if not home.is_dir():
        return []
    direct = _probe_root(home, probe)
    if direct:
        return [direct[1]]

    found: list[tuple[Path, Path]] = []

    def visit(root: Path, depth: int) -> None:
        for child in root.iterdir():
            if not child.is_dir():
                continue
            match = _probe_root(child, probe)
            if match:
                found.append(match)
            elif depth < 2:
                visit(child, depth + 1)

    visit(home, 0)
    return [include for _sdk, include in sorted(found, key=lambda item: _version_sort_key(item[0]))]


def find_include_dir(sdk: str | None = None, probe: str = "arkui/native_node.h") -> Path:
    """返回包含 probe（默认 arkui/native_node.h）的 include 根目录。"""
    tried: list[Path] = []

    if sdk:
        explicit = Path(sdk)
        roots = _discover_roots(explicit, probe)
        if roots:
            return roots[0]
        raise SystemExit(
            f"error: 显式 SDK 路径不包含 {probe}：{explicit}"
        )

    homes: list[Path] = []
    for var in ("OHOS_SDK_BASE", "OHOS_SDK_HOME", "OHSDK_HOME"):
        v = os.getenv(var)
        if v:
            homes.append(Path(v))
    homes.extend(Path(d) for d in DEVECO_SDKS)
    homes.append(Path(DEFAULT_SDK_HOME))

    for home in homes:
        roots = _discover_roots(home, probe)
        if roots:
            return roots[0]
        tried.append(home)
        if home in homes[:3]:
            warnings.warn(
                f"SDK candidate does not contain {probe}: {home}",
                RuntimeWarning,
                stacklevel=2,
            )

    checked = "; ".join(str(path) for path in tried) or "(empty)"
    raise SystemExit(
        f"error: 未找到 NDK 头文件（{probe}）。用 --sdk 指定 SDK 根，或设置 OHOS_SDK_BASE/OHSDK_HOME。"
        f"（已尝试根目录: {checked}）"
    )
