#ifndef MINI_ENUMS_H
#define MINI_ENUMS_H
#include "helper.h"

typedef enum {
    ARKUI_COLOR_RED = 0,
    ARKUI_COLOR_GREEN = 1,
} ArkUI_Color;

typedef enum {
    UI_TOUCH_EVENT_ACTION_CANCEL = 0,
    UI_TOUCH_EVENT_ACTION_DOWN = (1 << 1),
} UI_TouchEventAction;

#endif
