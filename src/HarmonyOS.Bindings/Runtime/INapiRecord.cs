#if HARMONYOS
namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// 可封送为 JS 对象的 record 契约。
/// 由绑定生成器为每个 Options record 生成显式实现（逐属性 napi_set_named_property），
/// 避免反射序列化在 NativeAOT 下的裁剪与启动开销问题。
/// </summary>
internal interface INapiRecord
{
    /// <summary>
    /// 将自身属性写入指定的 JS 对象
    /// </summary>
    void WriteTo(NativeNodeApi.napi_env env, NativeNodeApi.napi_value obj);
}
#endif
