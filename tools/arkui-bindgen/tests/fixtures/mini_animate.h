#ifndef MINI_ANIMATE_H
#define MINI_ANIMATE_H
/* 注意：fixture 不带系统 include（测试环境无 sysroot），
   只用 C 内建拼写（int / void / _Bool），否则 libclang 在无
   stdint.h 时会把函数指针成员解析成坏类型。 */

typedef enum { CB_DONE = 0, CB_FAIL = 1 } Mini_CallbackType;

typedef struct Mini_Context* Mini_ContextHandle;
typedef struct Mini_Opaque Mini_Opaque;

typedef struct {
    void* userData;
    void (*callback)(void* userData);
} Mini_Callback;

typedef struct {
    Mini_CallbackType type;
    void (*callback)(void* userData);
    void* userData;
} Mini_Complete;

typedef struct {
    int (*doIt)(Mini_ContextHandle ctx, Mini_Opaque* opt, Mini_Callback* update, Mini_Complete* complete);
    void (*dispose)(Mini_ContextHandle ctx);
} Mini_Api_1;

Mini_Opaque* OH_Mini_Create(void);
void OH_Mini_SetFlag(Mini_Opaque* opt, _Bool end);
void OH_Mini_SetCurve(Mini_Opaque* opt, Mini_CallbackType value);

#endif
