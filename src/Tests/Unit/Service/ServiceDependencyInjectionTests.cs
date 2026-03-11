using Microsoft.Extensions.DependencyInjection;

namespace DinExApi.Tests;

public sealed class ServiceDependencyInjectionTests
{
    [Fact]
    public void AddService_Should_Register_Main_Dispatcher_And_Handlers()
    {
        var services = new ServiceCollection();

        services.AddService();

        Assert.Contains(services, x => x.ServiceType == typeof(IApplicationDispatcher));
        Assert.Contains(services, x => x.ServiceType == typeof(IAssetAliasResolver));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<UpsertAssetDefinitionCommand, OperationResult<Guid>>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<DeleteAssetDefinitionCommand, OperationResult>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<RegisterUserCommand, OperationResult<Guid>>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<AuthenticateUserCommand, OperationResult<AuthenticatedUserResult>>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<ClearAllEntriesCommand, OperationResult>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<ClearCorporateEventsCommand, OperationResult>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<ImportInvestmentsSpreadsheetCommand, OperationResult<ImportInvestmentsSpreadsheetResult>>));
        Assert.Contains(services, x => x.ServiceType == typeof(ICommandHandler<ReconcilePortfolioCommand, OperationResult<ReconcilePortfolioResult>>));
        Assert.Contains(services, x => x.ServiceType == typeof(IQueryHandler<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>));
        Assert.Contains(services, x => x.ServiceType == typeof(IQueryHandler<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>));
        Assert.Contains(services, x => x.ServiceType == typeof(IQueryHandler<GetAssetDefinitionsQuery, OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>));
        Assert.Contains(services, x => x.ServiceType == typeof(IQueryHandler<GetStatementEntriesQuery, OperationResult<IReadOnlyCollection<StatementEntryItem>>>));
        Assert.Contains(services, x => x.ServiceType == typeof(IQueryHandler<GetCurrentUserQuery, OperationResult<CurrentUserItem>>));
    }
}
