using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// InterceptorType 枚举
/// </summary>
public enum InterceptorType
{
    [Description("INITIAL_REQUEST")]
    INITIAL_REQUEST,
    [Description("REDIRECTION")]
    REDIRECTION,
    [Description("READ_CACHE")]
    CACHE_CHECKED,
    [Description("CONNECT_NETWORK")]
    NETWORK_CONNECT,
    [Description("FINAL_RESPONSE")]
    FINAL_RESPONSE
}

/// <summary>
/// TlsVersion 枚举
/// </summary>
public enum TlsVersion
{
    TLS_V_1_0 = 4,
    TLS_V_1_1 = 5,
    TLS_V_1_2 = 6,
    TLS_V_1_3 = 7
}

/// <summary>
/// CertType 枚举
/// </summary>
public enum CertType
{
    [Description("PEM")]
    PEM,
    [Description("DER")]
    DER,
    [Description("P12")]
    P12
}

/// <summary>
/// AddressFamily 枚举
/// </summary>
public enum AddressFamily
{
    [Description("CURL_IPRESOLVE_WHATEVER")]
    DEFAULT,
    [Description("CURL_IPRESOLVE_V4")]
    ONLY_V4,
    [Description("CURL_IPRESOLVE_V6")]
    ONLY_V6
}

/// <summary>
/// RequestMethod 枚举
/// </summary>
public enum RequestMethod
{
    [Description("OPTIONS")]
    OPTIONS,
    [Description("GET")]
    GET,
    [Description("HEAD")]
    HEAD,
    [Description("POST")]
    POST,
    [Description("PUT")]
    PUT,
    [Description("DELETE")]
    DELETE,
    [Description("TRACE")]
    TRACE,
    [Description("CONNECT")]
    CONNECT,
    [Description("PATCH")]
    PATCH
}

/// <summary>
/// ResponseCode 枚举
/// </summary>
public enum ResponseCode
{
    OK = 200,
    CREATED = 201,
    ACCEPTED = 202,
    NOT_AUTHORITATIVE = 203,
    NO_CONTENT = 204,
    RESET = 205,
    PARTIAL = 206,
    MULT_CHOICE = 300,
    MOVED_PERM = 301,
    MOVED_TEMP = 302,
    SEE_OTHER = 303,
    NOT_MODIFIED = 304,
    USE_PROXY = 305,
    BAD_REQUEST = 400,
    UNAUTHORIZED = 401,
    PAYMENT_REQUIRED = 402,
    FORBIDDEN = 403,
    NOT_FOUND = 404,
    BAD_METHOD = 405,
    NOT_ACCEPTABLE = 406,
    PROXY_AUTH = 407,
    CLIENT_TIMEOUT = 408,
    CONFLICT = 409,
    GONE = 410,
    LENGTH_REQUIRED = 411,
    PRECON_FAILED = 412,
    ENTITY_TOO_LARGE = 413,
    REQ_TOO_LONG = 414,
    UNSUPPORTED_TYPE = 415,
    RANGE_NOT_SATISFIABLE = 416,
    INTERNAL_ERROR = 500,
    NOT_IMPLEMENTED = 501,
    BAD_GATEWAY = 502,
    UNAVAILABLE = 503,
    GATEWAY_TIMEOUT = 504,
    VERSION = 505
}

/// <summary>
/// HttpProtocol 枚举
/// </summary>
public enum HttpProtocol
{
    HTTP1_1 = 0,
    HTTP2 = 1,
    HTTP3 = 2
}

/// <summary>
/// HttpDataType 枚举
/// </summary>
public enum HttpDataType
{
    STRING = 0,
    OBJECT = 1,
    ARRAY_BUFFER = 2
}