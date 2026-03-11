namespace DinExApi.Service;

public sealed class DeleteAssetDefinitionCommandHandler(
    IAssetDefinitionRepository assetDefinitionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAssetDefinitionCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(
        DeleteAssetDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();
        var existing = await assetDefinitionRepository.GetByIdAsync(command.UserId, command.AssetDefinitionId, cancellationToken);
        if (existing is null)
        {
            result.AddError("Asset definition was not found.");
            result.SetAsNotFound();
            return result;
        }

        await assetDefinitionRepository.DeleteAsync(command.UserId, command.AssetDefinitionId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
