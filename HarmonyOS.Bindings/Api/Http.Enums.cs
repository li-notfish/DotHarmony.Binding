using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// InterceptorType 枚举
/// </summary>
public enum InterceptorType
{
    [Description("INITIAL_REQUEST")]
    InitialRequest,
    [Description("REDIRECTION")]
    Redirection,
    [Description("READ_CACHE")]
    CacheChecked,
    [Description("CONNECT_NETWORK")]
    NetworkConnect,
    [Description("FINAL_RESPONSE")]
    FinalResponse
}

/// <summary>
/// TlsVersion 枚举
/// </summary>
public enum TlsVersion
{
    TlsV10 = 4,
    TlsV11 = 5,
    TlsV12 = 6,
    TlsV13 = 7
}

/// <summary>
/// CertType 枚举
/// </summary>
public enum CertType
{
    [Description("PEM")]
    Pem,
    [Description("DER")]
    Der,
    [Description("P12")]
    P12
}

/// <summary>
/// AddressFamily 枚举
/// </summary>
public enum AddressFamily
{
    [Description("CURL_IPRESOLVE_WHATEVER")]
    Default,
    [Description("CURL_IPRESOLVE_V4")]
    OnlyV4,
    [Description("CURL_IPRESOLVE_V6")]
    OnlyV6
}

/// <summary>
/// RequestMethod 枚举
/// </summary>
public enum RequestMethod
{
    [Description("OPTIONS")]
    Options,
    [Description("GET")]
    Get,
    [Description("HEAD")]
    Head,
    [Description("POST")]
    Post,
    [Description("PUT")]
    Put,
    [Description("DELETE")]
    Delete,
    [Description("TRACE")]
    Trace,
    [Description("CONNECT")]
    Connect,
    [Description("PATCH")]
    Patch
}

/// <summary>
/// ResponseCode 枚举
/// </summary>
public enum ResponseCode
{
    Ok = 200,
    Created = 201,
    Accepted = 202,
    NotAuthoritative = 203,
    NoContent = 204,
    Reset = 205,
    Partial = 206,
    MultChoice = 300,
    MovedPerm = 301,
    MovedTemp = 302,
    SeeOther = 303,
    NotModified = 304,
    UseProxy = 305,
    BadRequest = 400,
    Unauthorized = 401,
    PaymentRequired = 402,
    Forbidden = 403,
    NotFound = 404,
    BadMethod = 405,
    NotAcceptable = 406,
    ProxyAuth = 407,
    ClientTimeout = 408,
    Conflict = 409,
    Gone = 410,
    LengthRequired = 411,
    PreconFailed = 412,
    EntityTooLarge = 413,
    ReqTooLong = 414,
    UnsupportedType = 415,
    RangeNotSatisfiable = 416,
    InternalError = 500,
    NotImplemented = 501,
    BadGateway = 502,
    Unavailable = 503,
    GatewayTimeout = 504,
    Version = 505
}

/// <summary>
/// HttpProtocol 枚举
/// </summary>
public enum HttpProtocol
{
    Http11 = 0,
    Http2 = 1,
    Http3 = 2
}

/// <summary>
/// HttpDataType 枚举
/// </summary>
public enum HttpDataType
{
    String = 0,
    Object = 1,
    ArrayBuffer = 2
}