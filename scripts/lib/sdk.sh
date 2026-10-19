#!/usr/bin/env bash
# HarmonyOS SDK 探测共享库（deploy-hap.sh / check-harmony-env.sh 复用）。
# 只依赖 SDK 目录布局，不要求 hdc 一定存在：交叉编译/打包机可以没有 hdc。

harmony_package_file() {
    HARMONY_PACKAGE_FILE=""
    local root="$1" file
    for file in \
        "$root/ets/oh-uni-package.json" \
        "$root/oh-uni-package.json" \
        "$root/toolchains/oh-uni-package.json" \
        "$root/package.json"; do
        if [ -f "$file" ]; then
            HARMONY_PACKAGE_FILE="$file"
            return 0
        fi
    done
    return 1
}

harmony_read_sdk_version() {
    local root="$1" field value leaf
    HARMONY_SDK_VERSION="unknown"
    if harmony_package_file "$root"; then
        for field in platformVersion version; do
            value="$(sed -n "s/.*\"$field\"[[:space:]]*:[[:space:]]*\"\\([^\"]*\\)\".*/\\1/p" "$HARMONY_PACKAGE_FILE" | head -n 1)"
            if [ -n "$value" ]; then
                HARMONY_SDK_VERSION="$value"
                return 0
            fi
        done
    fi

    leaf="${root##*/}"
    case "$leaf" in
        [0-9]*.[0-9]* | [0-9]*.[0-9]*-*) HARMONY_SDK_VERSION="$leaf" ;;
    esac
}

harmony_is_sdk_root() {
    local root="$1"
    [ -d "$root" ] || return 1
    [ -f "$root/ets/oh-uni-package.json" ] ||
        [ -f "$root/toolchains/hdc.exe" ] ||
        [ -f "$root/toolchains/hdc" ]
}

harmony_read_sdk_api_version() {
    local root="$1" value
    HARMONY_SDK_API_VERSION=""
    if harmony_package_file "$root"; then
        value="$(sed -n 's/.*"apiVersion"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$HARMONY_PACKAGE_FILE" | head -n 1)"
        [ -n "$value" ] || value="$(sed -n 's/.*"apiVersion"[[:space:]]*:[[:space:]]*\([0-9][0-9]*\).*/\1/p' "$HARMONY_PACKAGE_FILE" | head -n 1)"
        if [ -n "$value" ]; then
            HARMONY_SDK_API_VERSION="$value"
            return 0
        fi
    fi
    harmony_read_sdk_version "$root"
    case "$HARMONY_SDK_VERSION" in
        [0-9]*) HARMONY_SDK_API_VERSION="${HARMONY_SDK_VERSION%%.*}" ;;
        *) return 1 ;;
    esac
}

harmony_collect_roots() {
    local base="$1" child grandchild
    HARMONY_SDK_ROOTS=()
    [ -d "$base" ] || return 0
    if harmony_is_sdk_root "$base"; then
        HARMONY_SDK_ROOTS=("$base")
        return 0
    fi

    for child in "$base"/*; do
        [ -d "$child" ] || continue
        if harmony_is_sdk_root "$child"; then
            HARMONY_SDK_ROOTS+=("$child")
            continue
        fi
        for grandchild in "$child"/*; do
            [ -d "$grandchild" ] || continue
            if harmony_is_sdk_root "$grandchild"; then
                HARMONY_SDK_ROOTS+=("$grandchild")
            fi
        done
    done
}

harmony_sort_roots() {
    HARMONY_SORTED_SDK_ROOTS=("${HARMONY_SDK_ROOTS[@]}")
    [ "${#HARMONY_SORTED_SDK_ROOTS[@]}" -gt 1 ] || return 0

    local temp base sorted root version rank
    temp="$(mktemp)" || return 1
    base="${temp}.sorted"
    : > "$temp"
    for root in "${HARMONY_SDK_ROOTS[@]}"; do
        harmony_read_sdk_version "$root"
        version="$HARMONY_SDK_VERSION"
        case "$version" in
            *-*) rank=1 ;;
            [0-9]*) rank=0 ;;
            *) rank=2 ;;
        esac
        printf '%s\t%s\t%s\n' "$rank" "$version" "$root" >> "$temp"
    done
    LC_ALL=C sort -t $'\t' -k1,1n -k2,2Vr -k3,3Vr "$temp" -o "$base"
    HARMONY_SORTED_SDK_ROOTS=()
    while IFS=$'\t' read -r _rank _version root; do
        [ -n "$root" ] && HARMONY_SORTED_SDK_ROOTS+=("$root")
    done < "$base"
    rm -f "$temp" "$base"
}

harmony_sdk_candidates() {
    local deveco="${DEVECO_HOME:-}"
    local candidates=(
        "${OHOS_SDK_BASE:-}"
        "${OHOS_SDK_HOME:-}"
        "${OHSDK_HOME:-}"
        "${DEVECO_SDK_HOME:-}"
        "${HOS_SDK_HOME:-}"
        $([ -n "$deveco" ] && echo "$deveco/sdk")
        "$HOME/Library/Huawei/Sdk"
        "$HOME/Huawei/Sdk"
        "/mnt/d/Harmony/OpenHarmony/Sdk"
        "/mnt/c/Harmony/OpenHarmony/Sdk"
        "/mnt/d/Program Files/Huawei/DevEco Studio/sdk"
        "/mnt/c/Program Files/Huawei/DevEco Studio/sdk"
        "D:/Harmony/OpenHarmony/Sdk"
        "C:/Harmony/OpenHarmony/Sdk"
        "D:/Program Files/Huawei/DevEco Studio/sdk"
        "C:/Program Files/Huawei/DevEco Studio/sdk"
    )
    printf '%s\n' "${candidates[@]}"
}

harmony_find_sdk_root() {
    HARMONY_SDK_ROOT=""
    local candidate root
    local tried=()
    while IFS= read -r candidate; do
        [ -n "$candidate" ] || continue
        harmony_collect_roots "$candidate"
        if [ "${#HARMONY_SDK_ROOTS[@]}" -gt 0 ]; then
            harmony_sort_roots || return 1
            HARMONY_SDK_ROOT="${HARMONY_SORTED_SDK_ROOTS[0]}"
            return 0
        fi
        tried+=("$candidate")
    done < <(harmony_sdk_candidates)

    local checked="(none)"
    if [ "${#tried[@]}" -gt 0 ]; then
        checked="$(printf '%s, ' "${tried[@]}")"
        checked="${checked%, }"
    fi
    printf 'HarmonyOS/OpenHarmony SDK not found. Set OHOS_SDK_BASE/OHSDK_HOME/DEVECO_SDK_HOME. Checked: %s\n' "$checked" >&2
    return 1
}

harmony_find_hdc() {
    HDC=""
    if [ -n "${HARMONY_HDC:-}" ]; then
        if [ ! -f "$HARMONY_HDC" ]; then
            printf 'HARMONY_HDC does not exist: %s\n' "$HARMONY_HDC" >&2
            return 1
        fi
        HDC="$HARMONY_HDC"
        return 0
    fi

    harmony_find_sdk_root || return 1
    local candidate
    for candidate in "$HARMONY_SDK_ROOT/toolchains/hdc.exe" "$HARMONY_SDK_ROOT/toolchains/hdc"; do
        if [ -f "$candidate" ]; then
            HDC="$candidate"
            return 0
        fi
    done

    printf 'hdc not found under %s/toolchains. Set HARMONY_HDC if it lives elsewhere.\n' "$HARMONY_SDK_ROOT" >&2
    return 1
}

if [ "${BASH_SOURCE[0]}" != "$0" ]; then
    return 0
fi

case "${1:-}" in
    find-hdc)
        if harmony_find_hdc; then
            printf '%s\n' "$HDC"
            exit 0
        fi
        exit 1
        ;;
    sdk-root)
        if harmony_find_sdk_root; then
            printf '%s\n' "$HARMONY_SDK_ROOT"
            exit 0
        fi
        exit 1
        ;;
    *)
        printf 'usage: %s {find-hdc|sdk-root}\n' "${0##*/}" >&2
        exit 2
        ;;
esac
