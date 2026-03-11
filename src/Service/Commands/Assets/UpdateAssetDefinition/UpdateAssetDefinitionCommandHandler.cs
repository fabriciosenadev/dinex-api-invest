namespace DinExApi.Service;

public sealed class UpdateAssetDefinitionCommandHandler(
    IAssetDefinitionRepository assetDefinitionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAssetDefinitionCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(
        UpdateAssetDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();
        var existing = await assetDefinitionRepository.GetByIdAsync(command.UserId, command.AssetDefinitionId, cancellationToken);
        if (existing is null)
        {
            result.AddError("Asset definition was not found.");
            result.SetAsNotFound();
            return result;
        }

        var collision = await assetDefinitionRepository.GetBySymbolAsync(command.UserId, command.Symbol, cancellationToken);
        if (collision is not null && collision.Id != existing.Id)
        {
            result.AddError("Another asset definition already uses this symbol.");
            return result;
        }

        existing.Update(command.Symbol, command.Type, command.Notes);
        if (!existing.IsValid)
        {
            result.AddErrors(existing.Notifications.Select(x => x.Message));
            return result;
        }

        await assetDefinitionRepository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        result.SetData(existing.Id);
        return result;
    }
}
