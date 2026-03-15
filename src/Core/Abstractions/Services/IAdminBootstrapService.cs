namespace DinExApi.Core;

public interface IAdminBootstrapService
{
    Task EnsureAdminExistsAsync(CancellationToken cancellationToken = default);
}
