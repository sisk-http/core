using System.Reflection;

namespace Sisk.Mvc;

public static class AssemblyResourceHelper
{
    internal static Assembly EntryAssembly = Assembly.GetEntryAssembly()!;
    internal static string[] ManifestResourceNames = EntryAssembly.GetManifestResourceNames();

    public static Stream? GetAssemblyResourceStream(Assembly a, string filename)
    {
        if (filename.EndsWith(".hbs", StringComparison.OrdinalIgnoreCase) == false)
        {
            filename += ".hbs";
        }

        foreach (var r in ManifestResourceNames)
        {
            if (string.Compare(r[(r.IndexOf('.') + 1)..], filename, true) == 0)
            {
                return a.GetManifestResourceStream(r);
            }
        }

        return null;
    }

    public static Stream? GetAssemblyResourceStream(string filename) => GetAssemblyResourceStream(EntryAssembly, filename);
}
