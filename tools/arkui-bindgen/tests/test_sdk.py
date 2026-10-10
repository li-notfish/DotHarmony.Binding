"""SDK 探测回归测试：显式参数、环境回退、嵌套布局与版本排序。"""

import warnings
from pathlib import Path

import pytest

from arkui_bindgen.sdk import find_include_dir


def _touch_include(root: Path) -> Path:
    include = root / "native" / "sysroot" / "usr" / "include"
    include.mkdir(parents=True)
    (include / "probe.h").touch()
    return include


def _clear_sdk_environment(monkeypatch: pytest.MonkeyPatch) -> None:
    for name in ("OHOS_SDK_BASE", "OHOS_SDK_HOME", "OHSDK_HOME"):
        monkeypatch.delenv(name, raising=False)


def test_explicit_sdk_does_not_fall_back(tmp_path: Path, monkeypatch: pytest.MonkeyPatch):
    _clear_sdk_environment(monkeypatch)
    fallback = tmp_path / "fallback"
    _touch_include(fallback)
    monkeypatch.setenv("OHOS_SDK_BASE", str(fallback))

    with pytest.raises(SystemExit, match="显式 SDK 路径不包含"):
        find_include_dir(str(tmp_path / "missing"), probe="probe.h")


def test_invalid_environment_sdk_warns_and_falls_back(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
):
    _clear_sdk_environment(monkeypatch)
    invalid = tmp_path / "invalid"
    fallback = tmp_path / "fallback"
    expected = _touch_include(fallback)
    monkeypatch.setenv("OHOS_SDK_BASE", str(invalid))
    monkeypatch.setenv("OHOS_SDK_HOME", str(fallback))

    with warnings.catch_warnings(record=True) as caught:
        warnings.simplefilter("always")
        actual = find_include_dir(probe="probe.h")

    assert actual == expected
    assert any(
        item.category is RuntimeWarning and "probe.h" in str(item.message)
        for item in caught
    )


def test_nested_deveco_layout_is_discovered(tmp_path: Path):
    sdk_home = tmp_path / "sdk"
    expected = _touch_include(sdk_home / "default" / "openharmony" / "26.0.0")

    assert find_include_dir(str(sdk_home), probe="probe.h") == expected


def test_stable_sdk_sorts_before_newer_prerelease(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
):
    _clear_sdk_environment(monkeypatch)
    monkeypatch.setenv("OHOS_SDK_BASE", str(tmp_path))
    stable = _touch_include(tmp_path / "26.1.0")
    _touch_include(tmp_path / "27.0.0-beta1")
    _touch_include(tmp_path / "26.0.0")

    assert find_include_dir(probe="probe.h") == stable


def test_same_stability_uses_highest_semantic_version(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
):
    _clear_sdk_environment(monkeypatch)
    monkeypatch.setenv("OHOS_SDK_BASE", str(tmp_path))
    _touch_include(tmp_path / "26.1.0")
    expected = _touch_include(tmp_path / "26.10.0")
    _touch_include(tmp_path / "26.2.0")

    assert find_include_dir(probe="probe.h") == expected
