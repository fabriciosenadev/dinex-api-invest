namespace DinExApi.Service;

public sealed record GetIncomeTaxSummaryQuery(Guid UserId)
    : IQuery<OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>;
