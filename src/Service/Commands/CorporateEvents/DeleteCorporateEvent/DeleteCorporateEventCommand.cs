namespace DinExApi.Service;

public sealed record DeleteCorporateEventCommand(Guid UserId, Guid EventId) : ICommand<OperationResult<RegisterCorporateEventResult>>;
