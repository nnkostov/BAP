using BAP.Core.Models;

namespace BAP.Core.Interfaces;

/// <summary>
/// Creates the appropriate IHubConnection for a given hub model.
/// </summary>
public interface IHubConnectionFactory
{
    IHubConnection Create(HubModel hub);
}
