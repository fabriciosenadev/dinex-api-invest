namespace DinExApi.Service;

public sealed class GetCorporateEventsQueryHandler(ICorporateEventRepository corporateEventRepository)
    : IQueryHandler<GetCorporateEventsQuery, OperationResult<IReadOnlyCollection<CorporateEventItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<CorporateEventItem>>> HandleAsync(
        GetCorporateEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<CorporateEventItem>>();
        var items = await corporateEventRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        result.SetData(items
            .OrderByDescending(x => x.EffectiveAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new CorporateEventItem(
                x.Id,
                x.Type,
                x.SourceAssetSymbol,
                x.TargetAssetSymbol,
                x.Factor,
                x.EffectiveAtUtc,
                x.Notes,
                x.AppliedAtUtc))
            .ToArray());

        return result;
    }
}
