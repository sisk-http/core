using Cascadium;
using System.Reflection;
using System.Text;

namespace Sisk.Mvc;

public static class AssetsManager
{
    static string? css;

    public static (string css, string js) GetStaticAssets()
    {
        if (css is null)
        {
            var a = Assembly.GetEntryAssembly()!;
            var resources = a.GetManifestResourceNames()
                .OrderBy(a => a.Count(c => c == Path.DirectorySeparatorChar));

            StringBuilder styleBuilder = new StringBuilder();
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".xcss", StringComparison.OrdinalIgnoreCase))
                {
                    using Stream resourceStream = a.GetManifestResourceStream(resource)!;
                    using StreamReader sr = new StreamReader(resourceStream, Encoding.Default);

                    string xcss = sr.ReadToEnd();

#if DEBUG
                    string css = CascadiumCompiler.Compile(xcss, new CascadiumOptions() { Pretty = true });
                    styleBuilder.AppendLine($"/* {resource} */");
                    styleBuilder.AppendLine(css);
                    styleBuilder.AppendLine();
#else 
                    string css = CascadiumCompiler.Compile(xcss, new CascadiumOptions() { Pretty = false });
                    styleBuilder.Append(css);
#endif
                }
            }

            css = styleBuilder.ToString();
        }

        return (css, "");
    }
}
