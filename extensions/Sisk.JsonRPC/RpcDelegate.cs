using System.Reflection;

namespace Sisk.JsonRPC;

internal record class RpcDelegate(MethodInfo Method, object? Target);
