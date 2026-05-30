using BAP.Ble.Scripting;
using BAP.Core.Interfaces;
using BAP.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BAP.Ble;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BLE hub connection services, scripting engine, and section manager.
    /// The caller must also register an IBleAdapter implementation
    /// for the target platform (e.g. InTheHand.BluetoothLE).
    /// </summary>
    public static IServiceCollection AddBapBle(this IServiceCollection services)
    {
        services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
        services.AddSingleton<IScriptEngine, RoslynScriptEngine>();
        services.AddSingleton<SectionManager>();
        return services;
    }
}
