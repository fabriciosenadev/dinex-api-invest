namespace DinExApi.Service;

public sealed record ClearAllEntriesCommand(Guid UserId) : ICommand<OperationResult>;
