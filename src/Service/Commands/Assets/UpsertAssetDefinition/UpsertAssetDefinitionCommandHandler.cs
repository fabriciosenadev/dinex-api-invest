namespace DinExApi.Service;

public sealed class UpsertAssetDefinitionCommandHandler(
    IAssetDefinitionRepository assetDefinitionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpsertAssetDefinitionCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(
        UpsertAssetDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();
        var existing = await assetDefinitionRepository.GetBySymbolAsync(command.UserId, command.Symbol, cancellationToken);
        if (existing is not null)
        {
            existing.Update(
                command.Symbol,
                command.Type,
                command.Name,
                command.Document,
                command.Country,
                command.Currency,
                command.Sector,
                command.Segment,
                command.ShareClass,
                command.CvmCode,
                command.FiiCategory,
                command.Administrator,
                command.Manager,
                command.Notes);
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

        var assetDefinition = AssetDefinition.Create(
            command.UserId,
            command.Symbol,
            command.Type,
            command.Name,
            command.Document,
            command.Country,
            command.Currency,
            command.Sector,
            command.Segment,
            command.ShareClass,
            command.CvmCode,
            command.FiiCategory,
            command.Administrator,
            command.Manager,
            command.Notes);
        if (!assetDefinition.IsValid)
        {
            result.AddErrors(assetDefinition.Notifications.Select(x => x.Message));
            return result;
        }

        await assetDefinitionRepository.AddAsync(assetDefinition, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        result.SetData(assetDefinition.Id);
        return result;
    }
}
