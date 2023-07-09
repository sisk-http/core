using Sisk.Provider;

namespace NativeAOT_Test;

internal class Program
{
    static void Main(string[] args)
    {
        RouterEmitter routerEmitter = new RouterEmitter();
        ServiceProvider provider = new ServiceProvider(routerEmitter);
        provider.ConfigureInit(handler =>
        {
            handler.UseConfiguration(config =>
            {
                config.AccessLogsStream = null;
            });
            handler.UseLocale(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        });
    }
}
