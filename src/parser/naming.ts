/**
 * C# 命名规范化（.NET 风格标准化核心）。
 *
 * 规则依据 .NET Framework Design Guidelines：
 * - 多词 PascalCase（camelCase / SCREAMING_SNAKE / 混合输入统一切词）
 * - 三字母及以上缩写词首字母大写（URI→Uri、HTTP→Http、ID→Id）
 * - 两字母缩写词保留全大写（IP、TV、SN、AV）
 */

/** 三字母及以上缩写词归一表：全大写 token → .NET 规范拼写 */
const ACRONYM_MAP: Record<string, string> = {
    ABI: 'Abi', API: 'Api', APN: 'Apn', BSSID: 'Bssid', CPU: 'Cpu', DPI: 'Dpi',
    DNS: 'Dns', EAP: 'Eap', GNSS: 'Gnss', GPU: 'Gpu', HD: 'HD', HTML: 'Html',
    HTTP: 'Http', HTTPS: 'Https', ID: 'Id', IMEI: 'Imei', IMSI: 'Imsi',
    JSON: 'Json', LAN: 'Lan', LED: 'Led', MD5: 'Md5', MIME: 'Mime', MIMETYPE: 'MimeType', MTU: 'Mtu',
    NFC: 'Nfc', OEM: 'Oem', OK: 'Ok', RAM: 'Ram', RSSI: 'Rssi', SDK: 'Sdk',
    SD: 'SD', SIM: 'Sim', SN: 'SN', SOC: 'Soc', SSID: 'Ssid', TCP: 'Tcp',
    TV: 'TV', UDISK: 'Udisk', UDID: 'Udid', UDP: 'Udp', URI: 'Uri', URL: 'Url',
    USB: 'Usb', UUID: 'Uuid', VPN: 'Vpn', WLAN: 'Wlan', XML: 'Xml',
};

/** 两字母缩写保留全大写 */
const TWO_LETTER_KEEP = new Set(['IP', 'AV', 'QR', 'AR', 'VR', 'AI']);

/**
 * 将 TS 标识符切词：camelCase 边界 + SCREAMING_SNAKE 下划线。
 * getURI → [get, URI]；getDnsAscii → [get, Dns, Ascii]；MIMETYPE_TEXT_PLAIN → [MIMETYPE, TEXT, PLAIN]
 */
function splitWords(name: string): string[] {
    const withoutUnderscores = name.replace(/_/g, ' ');
    const spaced = withoutUnderscores
        .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
        .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2');
    return spaced.split(/\s+/).filter(w => w.length > 0);
}

/** 单个 token 归一为 PascalCase（含缩写词处理） */
function pascalToken(token: string): string {
    const upper = token.toUpperCase();
    if (ACRONYM_MAP[upper]) return ACRONYM_MAP[upper];
    if (TWO_LETTER_KEEP.has(upper)) return upper;
    // 数字开头 token（如 2IN1）：原样小写化，靠调用方前缀词保证合法 C# 标识符
    if (/^[0-9]/.test(token)) return token.toLowerCase();
    // 全大写的三字母以上缩写（不在表中）→ 首字母大写；其余首字母大写
    return token.charAt(0).toUpperCase() + token.slice(1).toLowerCase();
}

/**
 * TS 标识符 → C# PascalCase 成员名。
 * getURI → GetUri；getDnsAscii → GetDnsAscii；sdkApiVersion → SdkApiVersion；
 * TYPE_DEFAULT → TypeDefault；MIMETYPE_TEXT_PLAIN → MimeTypeTextPlain
 */
export function toPascalCase(name: string): string {
    return splitWords(name).map(pascalToken).join('');
}

/**
 * Task/Task<T> 返回类型的方法名补 Async 后缀（已有 Async 不重复）。
 */
export function withAsyncSuffix(pascalName: string, returnType: string): string {
    if (!/^Task(<.+>)?$/.test(returnType)) return pascalName;
    return pascalName.endsWith('Async') ? pascalName : `${pascalName}Async`;
}
