namespace DinExApi.Service;

public sealed record ClearCorporateEventsCommand(Guid UserId) : ICommand<OperationResult>;
