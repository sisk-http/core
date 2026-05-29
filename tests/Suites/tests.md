### 1. Capabilities Suite (`CapabilitiesSuite.md`)

# Suite: Capabilities
Esta suíte testa funcionalidades opcionais mas recomendadas do HTTP/1.1, focando em cache e conformidade de ETags.

| ID | Descrição |
|---|---|
| CAP-ETAG-304 | ETag conditional GET returns 304 Not Modified |
| CAP-LAST-MODIFIED-304 | Last-Modified conditional GET returns 304 Not Modified |
| CAP-ETAG-IN-304 | 304 response includes ETag header |
| CAP-INM-PRECEDENCE | If-None-Match takes precedence over If-Modified-Since |
| CAP-INM-WILDCARD | If-None-Match: * on existing resource returns 304 |
| CAP-IMS-FUTURE | If-Modified-Since with future date ignored |
| CAP-IMS-INVALID | If-Modified-Since with garbage date ignored |
| CAP-INM-UNQUOTED | If-None-Match with unquoted ETag |
| CAP-ETAG-WEAK | Weak ETag comparison for GET |

---

### 2. Compliance Suite (`ComplianceSuite.md`)

# Suite: Compliance
Esta suíte valida a conformidade estrita com as RFCs 9110 e 9112, cobrindo parsing de headers, limites de linha e comportamento padrão do servidor.

| ID | Descrição |
|---|---|
| COMP-BASELINE | Valid GET request — confirms server is reachable |
| RFC9112-2.2-BARE-LF-REQUEST-LINE | Bare LF in request line should be rejected, but MAY be accepted |
| RFC9112-2.2-BARE-LF-HEADER | Bare LF in header should be rejected, but MAY be accepted |
| RFC9112-5.1-OBS-FOLD | Obs-fold (line folding) in headers should be rejected |
| RFC9110-5.6.2-SP-BEFORE-COLON | Whitespace between header name and colon must be rejected |
| RFC9112-3-MULTI-SP-REQUEST-LINE | Multiple spaces between request-line components — SHOULD reject but MAY parse leniently |
| RFC9112-7.1-MISSING-HOST | Request without Host header must be rejected with 400 |
| RFC9112-2.3-INVALID-VERSION | Invalid HTTP version must be rejected |
| RFC9112-5-EMPTY-HEADER-NAME | Empty header name (leading colon) must be rejected |
| RFC9112-3-CR-ONLY-LINE-ENDING | CR without LF as line ending must be rejected |
| RFC9112-3-MISSING-TARGET | Request line with no target (space but no path) must be rejected |
| RFC9112-3.2-FRAGMENT-IN-TARGET | Fragment (#) in request-target — not part of origin-form grammar |
| RFC9112-2.3-HTTP09-REQUEST | HTTP/0.9 request (no version) must be rejected |
| RFC9112-5-INVALID-HEADER-NAME | Header name with invalid characters (brackets) must be rejected |
| RFC9112-5-HEADER-NO-COLON | Header line without colon must be rejected |
| RFC9110-5.4-DUPLICATE-HOST | Duplicate Host headers with different values must be rejected |
| RFC9112-6.1-CL-NON-NUMERIC | Non-numeric Content-Length must be rejected |
| RFC9112-6.1-CL-PLUS-SIGN | Content-Length with plus sign must be rejected |
| COMP-WHITESPACE-BEFORE-HEADERS | Whitespace before first header line must be rejected |
| COMP-DUPLICATE-HOST-SAME | Duplicate Host headers with identical values must be rejected |
| COMP-HOST-WITH-USERINFO | Host header with userinfo (user@host) must be rejected |
| COMP-HOST-WITH-PATH | Host header with path component must be rejected |
| COMP-ASTERISK-WITH-GET | Asterisk-form (*) request-target with GET must be rejected |
| COMP-OPTIONS-STAR | OPTIONS * is the only valid asterisk-form request |
| COMP-UNKNOWN-TE-501 | Unknown Transfer-Encoding without CL should be rejected with 501 |
| COMP-LEADING-CRLF | Leading CRLF before request-line — server may ignore per RFC |
| COMP-ABSOLUTE-FORM | Absolute-form request-target — server should accept per RFC |
| COMP-METHOD-CASE | Lowercase method 'get' — methods are case-sensitive per RFC |
| COMP-POST-CL-BODY | POST with Content-Length and matching body must be accepted |
| COMP-POST-CL-ZERO | POST with Content-Length: 0 and no body must be accepted |
| COMP-POST-NO-CL-NO-TE | POST with neither Content-Length nor Transfer-Encoding — implicit zero-length body |
| COMP-POST-CL-UNDERSEND | POST with Content-Length: 10 but only 5 bytes sent — incomplete body |
| COMP-CHUNKED-BODY | Valid single-chunk POST must be accepted |
| COMP-CHUNKED-MULTI | Valid multi-chunk POST must be accepted |
| COMP-CHUNKED-EMPTY | Zero-length chunked body (just terminator) must be accepted |
| COMP-CHUNKED-NO-FINAL | Chunked body without zero terminator — incomplete transfer |
| COMP-METHOD-CONNECT | CONNECT to an origin server must be rejected |
| COMP-EXPECT-UNKNOWN | Unknown Expect value should be rejected with 417 |
| COMP-GET-WITH-CL-BODY | GET with Content-Length and body — semantically unusual |
| COMP-CHUNKED-EXTENSION | Chunk extension (valid per RFC) — server should accept or may reject |
| COMP-METHOD-TRACE | TRACE request — should be disabled in production |
| COMP-HOST-EMPTY-VALUE | Empty Host header value must be rejected |
| COMP-REQUEST-LINE-TAB | Tab as request-line delimiter — SHOULD reject but MAY parse on whitespace |
| COMP-VERSION-MISSING-MINOR | HTTP/1 with no minor version digit is invalid |
| COMP-VERSION-LEADING-ZEROS | HTTP/01.01 — leading zeros in version digits are invalid |
| COMP-VERSION-WHITESPACE | HTTP/ 1.1 — whitespace inside version token is invalid |
| COMP-CONNECTION-CLOSE | Server must close connection after responding to Connection: close |
| COMP-HTTP10-DEFAULT-CLOSE | HTTP/1.0 without keep-alive — server should close connection after response |
| COMP-HTTP10-NO-HOST | HTTP/1.0 without Host header — valid per HTTP/1.0 |
| COMP-HTTP12-VERSION | HTTP/1.2 — higher minor version should be accepted as HTTP/1.x compatible |

---

### 3. Cookie Suite (`CookieSuite.md`)

# Suite: Cookie
Esta suíte foca no tratamento de cookies, abrangendo separadores, aspas e conformidade com a RFC 6265.

| ID | Descrição |
|---|---|
| COOKIE-COMMA-SEP | Cookies separated by comma in Set-Cookie must be handled correctly |
| COOKIE-QUOTED-VALUE | Quoted cookie values must be accepted and parsed correctly |
| COOKIE-SEMICOLON-IN-VALUE | Semicolon inside quoted cookie value |
| COOKIE-TRAILING-SEMICOLON | Set-Cookie with trailing semicolon |
| COOKIE-SPACE-BEFORE-VALUE | Cookie with space between name and value |
| COOKIE-MULTIPLE-ATTRIBUTES | Cookie with multiple attributes (Max-Age, Path, HttpOnly) |
| COOKIE-MALFORMED-DATE | Cookie with invalid Expires date |
| COOKIE-OBSOLETE-FOLD | Cookie values with obsolete line folding (obs-fold) |
| COOKIE-NON-ASCII | Cookie values with non-ASCII characters |
| COOKIE-EMPTY-NAME | Cookie with empty name (=value) |
| COOKIE-EMPTY-VALUE | Cookie with empty value (name=) |

---

### 4. Malformed Input Suite (`MalformedInputSuite.md`)

# Suite: Malformed Input
Esta suíte testa a robustez do servidor contra entradas malformadas ou propositalmente inválidas que visam quebrar o parser HTTP.

| ID | Descrição |
|---|---|
| MALF-BOM-START | Request starting with UTF-8 Byte Order Mark (BOM) |
| MALF-NULL-BYTE-IN-HEADER | Null byte (0x00) inside header name or value |
| MALF-NULL-BYTE-IN-PATH | Null byte (0x00) inside request-target (path) |
| MALF-CTL-IN-HEADER | Control characters (0x01-0x1F) in header values |
| MALF-INVALID-UTF8-PATH | Invalid UTF-8 sequence in request-target |
| MALF-VERY-LONG-REQUEST-LINE | Extremely long request line (e.g., 1MB) |
| MALF-VERY-LONG-HEADER | Single header line exceeding typical limits (e.g., 64KB) |
| MALF-TOO-MANY-HEADERS | Request with thousands of small headers |
| MALF-NEGATIVE-CONTENT-LENGTH | Content-Length with negative value |
| MALF-OVERFLOW-CONTENT-LENGTH | Content-Length larger than UInt64 max value |
| MALF-MULTIPLE-CL-DIFFERENT | Conflicting Content-Length headers with different values |
| MALF-CL-AND-TE | Both Content-Length and Transfer-Encoding: chunked present |
| MALF-INVALID-CHUNK-SIZE | Non-hexadecimal chunk size in chunked transfer |
| MALF-CHUNK-SIZE-TOO-LARGE | Chunk size indicating more data than available in memory |

---

### 5. Normalization Suite (`NormalizationSuite.md`)

# Suite: Normalization
Esta suíte valida como o servidor normaliza URIs e caminhos, crucial para segurança e roteamento correto.

| ID | Descrição |
|---|---|
| NORM-DOT-PATH | Path with single dot segments (/./a) |
| NORM-DOUBLE-DOT-PATH | Path with double dot segments (/a/b/../c) |
| NORM-DIR-TRAVERSAL | Attempted path traversal via escaping root (/../../etc/passwd) |
| NORM-ENCODED-DOT | URL encoded dot (%2e) and double dot (%2e%2e) segments |
| NORM-ENCODED-SLASH | URL encoded slash (%2f) in path |
| NORM-DOUBLE-SLASH | Multiple consecutive slashes (//) in path |
| NORM-TRAILING-DOT | Path ending in a dot segment (/a/.) |
| NORM-CASE-INSENSITIVITY | Verifies if path segments are treated case-insensitively (if applicable) |
| NORM-NON-UTF8-PCT | Percent encoding with non-UTF8 sequences |

---

### 6. Smuggling Suite (`SmugglingSuite.md`)

# Suite: Smuggling (Request Smuggling & Desync)
Esta suíte é dedicada a testar vulnerabilidades de Request Smuggling (CL.TE, TE.CL) e dessincronização de protocolo. É a suíte mais extensa.

| ID | Descrição |
|---|---|
| SMUG-TE-TRANSFER_ENCODING | Transfer-Encoding with underscore (Transfer_Encoding) |
| SMUG-TRANSFER_ENCODING | Transfer_Encoding (underscore) header with CL — not a valid header but some parsers accept |
| SMUG-CL-COMMA-SAME | Content-Length with comma-separated identical values — some servers merge |
| SMUG-CL-COMMA-TRIPLE | Content-Length with three comma-separated identical values — extended merge test |
| SMUG-CHUNKED-WITH-PARAMS | Transfer-Encoding: chunked;ext=val — parameters on chunked encoding |
| SMUG-EXPECT-100-CL | Expect: 100-continue with Content-Length — server should send 100 then read body |
| SMUG-TRAILER-CL | Content-Length in chunked trailers must be ignored — prohibited trailer field |
| SMUG-TRAILER-TE | Transfer-Encoding in chunked trailers must be ignored — prohibited trailer field |
| SMUG-TRAILER-HOST | Host header in chunked trailers must not be used for routing |
| SMUG-TRAILER-AUTH | Authorization header in chunked trailers — prohibited per RFC 9110 §6.5.1 |
| SMUG-HEAD-CL-BODY | HEAD request with Content-Length and body — server must not leave body on connection |
| SMUG-OPTIONS-CL-BODY | OPTIONS with Content-Length and body — server should consume or reject body |
| SMUG-CL-UNDERSCORE | Content-Length with underscore digit separator (1_0) must be rejected |
| SMUG-CL-NEGATIVE-ZERO | Content-Length: -0 must be rejected — not valid 1*DIGIT |
| SMUG-CL-DOUBLE-ZERO | Content-Length: 00 — matches 1*DIGIT but leading zero ambiguity |
| SMUG-CL-LEADING-ZEROS-OCTAL | Content-Length: 0200 — octal 128 vs decimal 200, parser disagreement vector |
| SMUG-TE-OBS-FOLD | Transfer-Encoding with obs-fold line wrapping must be rejected |
| SMUG-TE-TRAILING-COMMA | Transfer-Encoding: chunked, — trailing comma produces empty list element |
| SMUG-TE-TAB-BEFORE-VALUE | Transfer-Encoding with tab as OWS before value |
| SMUG-ABSOLUTE-URI-HOST-MISMATCH | Absolute-form URI with different Host header — routing confusion vector |
| SMUG-MULTIPLE-HOST-COMMA | Host header with comma-separated values must be rejected |
| SMUG-CHUNK-BARE-CR-TERM | Chunk size line terminated by bare CR — not a valid line terminator |
| SMUG-TRAILER-CONTENT-TYPE | Content-Type in chunked trailer — prohibited per RFC 9110 §6.5.1 |
| SMUG-CLTE-CONN-CLOSE | CL+TE conflict — server MUST close connection after responding |
| SMUG-TECL-CONN-CLOSE | TE+CL conflict (reversed order) — server MUST close connection after responding |
| SMUG-CLTE-DESYNC | CL.TE desync — leftover bytes after the body boundary may be interpreted as the next request |
| SMUG-CLTE-SMUGGLED-GET | CL.TE desync — embedded GET in body; multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-CL-PLUS | CL.TE desync with malformed Content-Length (+N) — multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-CL-NON-NUMERIC | CL.TE desync with non-numeric Content-Length (N<alpha>) — multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-TE-OBS-FOLD | CL.TE desync with obs-folded Transfer-Encoding — multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-HEAD | CL.TE desync — embedded HEAD in body; multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-TE-TRAILING-SPACE | CL.TE desync with TE trailing space — multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-TE-LEADING-COMMA | CL.TE desync with TE leading comma — multiple responses indicate request boundary confusion |
| SMUG-CLTE-SMUGGLED-GET-TE-CASE-MISMATCH | CL.TE desync with TE case mismatch — multiple responses indicate request boundary confusion |
| SMUG-TE-DUPLICATE-HEADERS-SMUGGLED-GET | TE.TE + CL ambiguity with embedded GET — multiple responses indicate request boundary confusion |
| SMUG-TECL-SMUGGLED-GET | TE.CL desync via chunk-size prefix trick — multiple responses indicate request boundary confusion |
| SMUG-DUPLICATE-CL-SMUGGLED-GET | CL.CL ambiguity with embedded GET — multiple responses indicate request boundary confusion |
| SMUG-GET-CL-PREFIX-DESYNC | GET with Content-Length body containing an incomplete request prefix — follow-up completes it if body was left unread |
| SMUG-TECL-DESYNC | TE.CL desync — chunked terminator before CL boundary, leftover bytes smuggled |
| SMUG-CL0-BODY-POISON | Content-Length: 0 with trailing bytes — checks if leftover bytes poison the next request |
| SMUG-GET-CL-BODY-DESYNC | GET with Content-Length body followed by a second request — detects unread-body desync |
| SMUG-OPTIONS-CL-BODY-DESYNC | OPTIONS with Content-Length body followed by a second request — detects unread-body desync |
| SMUG-EXPECT-100-CL-DESYNC | Expect: 100-continue with immediate body followed by a second request — detects unread-body desync |
| SMUG-OPTIONS-TE-OBS-FOLD | OPTIONS with TE obs-fold and CL present — server must reject or close after response |
| SMUG-CHUNK-INVALID-SIZE-DESYNC | Invalid chunk size (+0) with poison byte — detects chunk-size parser desync |
| SMUG-PIPELINE-SAFE | Baseline — two clean GET requests on one keep-alive connection |

---

### 7. WebSockets Suite (`WebSocketsSuite.md`)

# Suite: WebSockets
Esta suíte valida a segurança e conformidade do handshake de upgrade para WebSockets (RFC 6455).

| ID | Descrição |
|---|---|
| WS-UPGRADE-POST | WebSocket upgrade via POST must not be accepted |
| WS-UPGRADE-MISSING-CONN | Upgrade header without Connection: Upgrade must not trigger protocol switch |
| WS-UPGRADE-UNKNOWN | Upgrade to unknown protocol must not return 101 |
| WS-UPGRADE-INVALID-VER | WebSocket upgrade with unsupported version — should return 426 |
| WS-UPGRADE-HTTP10 | Upgrade header in HTTP/1.0 request must be ignored |