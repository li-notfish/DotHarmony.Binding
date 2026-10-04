#!/usr/bin/env bash
set -u
mode="${FAKE_HDC_MODE:?FAKE_HDC_MODE is required}"

if [ "${1:-}" = "-t" ]; then shift 2; fi

case "${1:-} ${2:-}" in
    "list targets")
        if [ "$mode" = "no-device" ]; then printf '[Empty]\n'; else printf '127.0.0.1:5555\n'; fi
        exit 0
        ;;
esac

case "${1:-}" in
    install)
        printf 'install bundle successfully\n'
        exit 0
        ;;
    shell)
        command="${2:-}"
        case "$command" in
            "pidof "*)
                [ "$mode" = "success" ] && printf '4242\n'
                exit 0
                ;;
            "hilog -x")
                [ "$mode" = "success" ] && printf '01-01 00:00:00.000 4242 4242 I A00000/HarmonyHost: Ability onCreate\n'
                exit 0
                ;;
            "aa start "*)
                printf 'start ability successfully.\n'
                exit 0
                ;;
        esac
        ;;
esac
exit 0
