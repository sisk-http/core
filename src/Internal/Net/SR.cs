// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal;
partial class SR
{
    // from System.Net.HttpListeners resx

    public const string net_log_listener_delegate_exception = "Sending 500 response, AuthenticationSchemeSelectorDelegate threw an exception: {0}.";
    public const string net_log_listener_unsupported_authentication_scheme = "Received a request with an unsupported authentication scheme, Authorization:{0} SupportedSchemes:{1}.";
    public const string net_log_listener_unmatched_authentication_scheme = "Received a request with an unmatched or no authentication scheme. AuthenticationSchemes:{0}, Authorization:{1}.";
    public const string net_io_invalidasyncresult = "The IAsyncResult object was not returned from the corresponding asynchronous method on this class.";
    public const string net_io_invalidendcall = "{0} can only be called once for each asynchronous operation.";
    public const string net_io_out_range = "The byte count must not exceed {0} bytes for this stream type.";
    public const string net_listener_cannot_set_custom_cbt = "Custom channel bindings are not supported.";
    public const string net_listener_detach_error = "Can't detach Url group from request queue. Status code: {0}.";
    public const string net_listener_scheme = "Only Uri prefixes starting with 'http://' or 'https://' are supported.";
    public const string net_listener_host = "Only Uri prefixes with a valid hostname are supported.";
    public const string net_listener_not_supported = "The request is not supported.";
    public const string net_listener_mustcall = "Please call the {0} method before calling this method.";
    public const string net_listener_slash = "Only Uri prefixes ending in '/' are allowed.";
    public const string net_listener_already = "Failed to listen on prefix '{0}' because it conflicts with an existing registration on the machine.";
    public const string net_log_listener_no_cbt_disabled = "No channel binding check because extended protection is disabled.";
    public const string net_log_listener_no_cbt_http = "No channel binding check for requests without a secure channel.";
    public const string net_log_listener_no_cbt_trustedproxy = "No channel binding check for the trusted proxy scenario.";
    public const string net_log_listener_cbt = "Channel binding check enabled.";
    public const string net_log_listener_no_spn_kerberos = "No explicit service name check because Kerberos authentication already validates the service name.";
    public const string net_log_listener_no_spn_disabled = "No service name check because extended protection is disabled.";
    public const string net_log_listener_no_spn_cbt = "No service name check because the channel binding was already checked.";
    public const string net_log_listener_no_spn_whensupported = "No service name check because the client did not provide a service name and the server was configured for PolicyEnforcement.WhenSupported.";
    public const string net_log_listener_no_spn_loopback = "No service name check because the authentication was from a client on the local machine.";
    public const string net_log_listener_spn = "Client provided service name '{0}'.";
    public const string net_log_listener_spn_passed = "Service name check succeeded.";
    public const string net_log_listener_spn_failed = "Service name check failed.";
    public const string net_log_listener_spn_failed_always = "Service name check failed because the client did not provide a service name and the server was configured for PolicyEnforcement.Always.";
    public const string net_log_listener_spn_failed_empty = "No acceptable service names were configured!";
    public const string net_log_listener_spn_failed_dump = "Dumping acceptable service names:";
    public const string net_log_listener_spn_add = "Adding default service name '{0}' from prefix '{1}'.";
    public const string net_log_listener_spn_not_add = "No default service name added for prefix '{0}'.";
    public const string net_log_listener_spn_remove = "Removing default service name '{0}' from prefix '{1}'.";
    public const string net_log_listener_spn_not_remove = "No default service name removed for prefix '{0}'.";
    public const string net_listener_no_spns = "No service names could be determined from the registered prefixes. Either add prefixes from which default service names can be derived or specify an ExtendedProtectionPolicy object which contains an explicit list of service names.";
    public const string net_ssp_dont_support_cbt = "The Security Service Providers don't support extended protection. Please install the latest Security Service Providers update.";
    public const string net_PropertyNotImplementedException = "This property is not implemented by this class.";
    public const string net_array_too_small = "The target array is too small.";
    public const string net_listener_mustcompletecall = "The in-progress method {0} must be completed first.";
    public const string net_listener_invalid_cbt_type = "Querying the {0} Channel Binding is not supported.";
    public const string net_listener_callinprogress = "Cannot re-call {0} while a previous call is still in progress.";
    public const string net_log_listener_cant_create_uri = "Can't create Uri from string '{0}://{1}{2}{3}'.";
    public const string net_log_listener_cant_convert_raw_path = "Can't convert Uri path '{0}' using encoding '{1}'.";
    public const string net_log_listener_cant_convert_percent_value = "Can't convert percent encoded value '{0}'.";
    public const string net_log_listener_cant_convert_to_utf8 = "Can't convert string '{0}' into UTF-8 bytes: {1}";
    public const string net_log_listener_cant_convert_bytes = "Can't convert bytes '{0}' into UTF-16 characters: {1}";
    public const string net_invalidstatus = "The status code must be exactly three digits.";
    public const string net_WebHeaderInvalidControlChars = "Specified value has invalid Control characters.";
    public const string net_rspsubmitted = "This operation cannot be performed after the response has been submitted.";
    public const string net_nochunkuploadonhttp10 = "Chunked encoding upload is not supported on the HTTP/1.0 protocol.";
    public const string net_cookie_exists = "Cookie already exists.";
    public const string net_wrongversion = "Only HTTP/1.0 and HTTP/1.1 version requests are currently supported.";
    public const string net_noseek = "This stream does not support seek operations.";
    public const string net_writeonlystream = "The stream does not support reading.";
    public const string net_entitytoobig = "Bytes to be written to the stream exceed the Content-Length bytes size specified.";
    public const string net_io_notenoughbyteswritten = "Cannot close stream until all bytes are written.";
    public const string net_listener_close_urlgroup_error = "Can't close Url group. Status code: {0}.";
    public const string net_WebSockets_NativeSendResponseHeaders = "An error occurred when sending the WebSocket HTTP upgrade response during the {0} operation. The HRESULT returned is '{1}'";
    public const string net_WebSockets_ClientAcceptingNoProtocols = "The WebSocket client did not request any protocols, but server attempted to accept '{0}' protocol(s).";
    public const string net_WebSockets_AcceptUnsupportedProtocol = "The WebSocket client request requested '{0}' protocol(s), but server is only accepting '{1}' protocol(s).";
    public const string net_WebSockets_AcceptNotAWebSocket = "The {0} operation was called on an incoming request that did not specify a '{1}: {2}' header or the {2} header not contain '{3}'. {2} specified by the client was '{4}'.";
    public const string net_WebSockets_AcceptHeaderNotFound = "The {0} operation was called on an incoming WebSocket request without required '{1}' header.";
    public const string net_WebSockets_AcceptUnsupportedWebSocketVersion = "The {0} operation was called on an incoming request with WebSocket version '{1}', expected '{2}'.";
    public const string net_WebSockets_InvalidCharInProtocolString = "The WebSocket protocol '{0}' is invalid because it contains the invalid character '{1}'.";
    public const string net_WebSockets_ReasonNotNull = "The close status description '{0}' is invalid. When using close status code '{1}' the description must be null.";
    public const string net_WebSockets_InvalidCloseStatusCode = "The close status code '{0}' is reserved for system use only and cannot be specified when calling this method.";
    public const string net_WebSockets_InvalidCloseStatusDescription = "The close status description '{0}' is too long. The UTF-8 representation of the status description must not be longer than {1} bytes.";
    public const string net_WebSockets_UnsupportedPlatform = "The WebSocket protocol is not supported on this platform.";
    public const string net_readonlystream = "The stream does not support writing.";
    public const string net_WebSockets_InvalidState_ClosedOrAborted = "'{0}' instance cannot be used for communication because it has been transitioned into the '{1}' state.";
    public const string net_WebSockets_ReceiveAsyncDisallowedAfterCloseAsync = "The WebSocket is in an invalid state for this operation. The '{0}' method has already been called before on this instance. Use '{1}' instead to keep being able to receive data but close the output channel.";
    public const string net_Websockets_AlreadyOneOutstandingOperation = "There is already one outstanding '{0}' call for this WebSocket instance. ReceiveAsync and SendAsync can be called simultaneously, but at most one outstanding operation for each of them is allowed at the same time.";
    public const string net_WebSockets_InvalidMessageType = "The received message type '{2}' is invalid after calling {0}. {0} should only be used if no more data is expected from the remote endpoint. Use '{1}' instead to keep being able to receive data but close the output channel.";
    public const string net_WebSockets_InvalidBufferType = "The buffer type '{0}' is invalid. Valid buffer types are: '{1}', '{2}', '{3}', '{4}', '{5}'.";
    public const string net_WebSockets_ArgumentOutOfRange_InternalBuffer = "The byte array must have a length of at least '{0}' bytes.";
    public const string net_WebSockets_Argument_InvalidMessageType = "The message type '{0}' is not allowed for the '{1}' operation. Valid message types are: '{2}, {3}'. To close the WebSocket, use the '{4}' operation instead.";
    public const string net_securitypackagesupport = "The requested security package is not supported.";
    public const string net_log_operation_failed_with_error = "{0} failed with error {1}.";
    public const string net_MethodNotImplementedException = "This method is not implemented by this class.";
    public const string net_invalid_enum = "The specified value is not valid in the '{0}' enumeration.";
    public const string net_auth_message_not_encrypted = "Protocol error: A received message contains a valid signature but it was not encrypted as required by the effective Protection Level.";
    public const string SSPIInvalidHandleType = "'{0}' is not a supported handle type.";
    public const string net_io_operation_aborted = "I/O operation aborted: '{0}'.";
    public const string net_invalid_path = "Invalid path.";
    public const string net_listener_auth_errors = "Authentication errors.";
    public const string net_listener_close = "Listener closed.";
    public const string net_invalid_port = "Invalid port in prefix.";
    public const string net_WebSockets_InvalidState = "The WebSocket is in an invalid state ('{0}') for this operation. Valid states are: '{1}'";
    public const string SystemNetHttpListener_PlatformNotSupported = "System.Net.HttpListener is not supported on this platform.";
    public const string net_cookie_attribute = "The '{0}'='{1}' part of the cookie is invalid.";

}
