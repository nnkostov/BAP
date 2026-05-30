using BAP.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BAP.Ble;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers BLE hub connection services.
    /// The caller must also register an IBleAdapter implementation
    /// for the target platform (e.g. InTheHand.BluetoothLE).
    /// </summary>
    public static IServiceCollection AddBapBle(this IServiceCollection services)
    {
        services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
        return services;
    }
}
