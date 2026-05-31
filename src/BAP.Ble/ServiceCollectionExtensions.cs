using BAP.Ble.Scripting;
using BAP.Core.Interfaces;
using BAP.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        // Register NullBleAdapter as fallback if no platform adapter was registered
        services.TryAddSingleton<IBleAdapter, NullBleAdapter>();
        services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
        services.AddSingleton<IScriptEngine, RoslynScriptEngine>();
        services.AddSingleton<SectionManager>();
        return services;
    }
}
