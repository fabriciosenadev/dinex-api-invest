namespace DinExApi.Api.Contracts.Common;

public sealed record ErrorResponse(IReadOnlyCollection<string> Errors);
