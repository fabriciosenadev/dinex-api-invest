namespace DinExApi.Service;

public sealed class GetStatementEntriesQueryHandler(ILedgerEntryRepository repository)
    : IQueryHandler<GetStatementEntriesQuery, OperationResult<IReadOnlyCollection<StatementEntryItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<StatementEntryItem>>> HandleAsync(
        GetStatementEntriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<StatementEntryItem>>();

        try
        {
            var entries = await repository.GetByUserIdAsync(query.UserId, query.FromUtc, query.ToUtc, cancellationToken);
            var data = entries
                .Select(x => new StatementEntryItem(
                    x.Id,
                    x.Type,
                    x.Description,
                    x.AssetSymbol,
                    x.Quantity,
                    x.UnitPriceAmount,
                    x.GrossAmount,
                    x.NetAmount,
                    x.Currency,
                    x.OccurredAtUtc,
                    x.Source,
                    x.ReferenceId,
                    x.Metadata))
                .ToArray();

            result.SetData(data);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while loading statement entries.");
            return result;
        }
    }
}
