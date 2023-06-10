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
            handler.UseLocale(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        });
    }
}
