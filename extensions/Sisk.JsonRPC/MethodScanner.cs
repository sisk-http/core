using Sisk.JsonRPC.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Sisk.JsonRPC;

internal static class MethodScanner
{
    public static IEnumerable<(string, RpcDelegate)> ScanMethods([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, bool prefixTypes, object? target)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        var customTypeName = type.GetCustomAttribute<WebNameAttribute>()?.Name;
        var methods = type.GetMethods(Flags);
        for (int i = 0; i < methods.Length; i++)
        {
            var method = methods[i];
            WebMethodAttribute? methodName = method.GetCustomAttribute<WebMethodAttribute>();

            if (methodName is not null)
            {
                string typeName = customTypeName ?? type.Name;
                string name = methodName.Name ?? method.Name;

                if (prefixTypes)
                    name = typeName + "." + name;

                yield return (name, new RpcDelegate(method, target));
            }
        }
    }
}
