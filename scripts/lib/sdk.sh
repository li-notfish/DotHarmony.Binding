#!/usr/bin/env bash

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

harmony_collect_roots() {
    local base="$1" child grandchild
    HARMONY_SDK_ROOTS=()
    [ -d "$base" ] || return 0
    if [ -f "$base/toolchains/hdc.exe" ]; then
        HARMONY_SDK_ROOTS=("$base")
        return 0
    fi

    for child in "$base"/*; do
        [ -d "$child" ] || continue
        if [ -f "$child/toolchains/hdc.exe" ]; then
            HARMONY_SDK_ROOTS+=("$child")
            continue
        fi
        for grandchild in "$child"/*; do
            [ -d "$grandchild" ] || continue
            if [ -f "$grandchild/toolchains/hdc.exe" ]; then
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

harmony_find_hdc() {
    HDC=""
    local candidate root
    local tried=()
    if [ -n "${HARMONY_HDC:-}" ]; then
        if [ ! -f "$HARMONY_HDC" ]; then
            printf 'HARMONY_HDC does not exist: %s\n' "$HARMONY_HDC" >&2
            return 1
        fi
        HDC="$HARMONY_HDC"
        return 0
    fi
    local candidates=(
        "${OHOS_SDK_BASE:-}"
        "${OHOS_SDK_HOME:-}"
        "${OHSDK_HOME:-}"
        "${DEVECO_HOME:+$DEVECO_HOME/sdk}"
        "D:/Harmony/OpenHarmony/Sdk"
        "C:/Harmony/OpenHarmony/Sdk"
        "/mnt/d/Harmony/OpenHarmony/Sdk"
        "D:/Program Files/Huawei/DevEco Studio/sdk"
        "C:/Program Files/Huawei/DevEco Studio/sdk"
        "/mnt/d/Program Files/Huawei/DevEco Studio/sdk"
    )

    for candidate in "${candidates[@]}"; do
        [ -n "$candidate" ] || continue
        if [ ! -d "$candidate" ]; then
            printf 'SDK candidate does not exist: %s\n' "$candidate" >&2
            tried+=("$candidate")
            continue
        fi

        harmony_collect_roots "$candidate"
        if [ "${#HARMONY_SDK_ROOTS[@]}" -gt 0 ]; then
            harmony_sort_roots || return 1
            root="${HARMONY_SORTED_SDK_ROOTS[0]}"
            if [ -f "$root/toolchains/hdc.exe" ]; then
                HDC="$root/toolchains/hdc.exe"
                return 0
            fi
        fi
        tried+=("$candidate")
        printf 'SDK candidate has no toolchains/hdc.exe: %s\n' "$candidate" >&2
    done

    local checked
    if [ "${#tried[@]}" -gt 0 ]; then
        checked="$(printf '%s, ' "${tried[@]}")"
        checked="${checked%, }"
    else
        checked="(none)"
    fi
    printf 'hdc.exe not found. Set OHOS_SDK_BASE/OHSDK_HOME. Checked: %s\n' "$checked" >&2
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
        if harmony_find_hdc; then
            printf '%s\n' "$(dirname "$(dirname "$HDC")")"
            exit 0
        fi
        exit 1
        ;;
    *)
        printf 'usage: %s {find-hdc|sdk-root}\n' "${0##*/}" >&2
        exit 2
        ;;
esac
