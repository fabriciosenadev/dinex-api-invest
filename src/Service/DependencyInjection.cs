namespace DinExApi.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDispatcher, ApplicationDispatcher>();
        services.AddScoped<IAssetAliasResolver, JsonAssetAliasResolver>();
        services.AddScoped<IInvestmentMovementClassifier, B3InvestmentMovementClassifier>();
        services.AddScoped<IAdminBootstrapService, AdminBootstrapService>();
        services.AddScoped<ICommandHandler<UpsertAssetDefinitionCommand, OperationResult<Guid>>, UpsertAssetDefinitionCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateAssetDefinitionCommand, OperationResult<Guid>>, UpdateAssetDefinitionCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteAssetDefinitionCommand, OperationResult>, DeleteAssetDefinitionCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterMovementCommand, OperationResult<Guid>>, RegisterMovementCommandHandler>();
        services.AddScoped<ICommandHandler<ReconcilePortfolioCommand, OperationResult<ReconcilePortfolioResult>>, ReconcilePortfolioCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterStatementEntryCommand, OperationResult<Guid>>, RegisterStatementEntryCommandHandler>();
        services.AddScoped<ICommandHandler<ClearAllEntriesCommand, OperationResult>, ClearAllEntriesCommandHandler>();
        services.AddScoped<ICommandHandler<ImportInvestmentsSpreadsheetCommand, OperationResult<ImportInvestmentsSpreadsheetResult>>, ImportInvestmentsSpreadsheetCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>, RegisterCorporateEventCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>, UpdateCorporateEventCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>, DeleteCorporateEventCommandHandler>();
        services.AddScoped<ICommandHandler<ClearCorporateEventsCommand, OperationResult>, ClearCorporateEventsCommandHandler>();
        services.AddScoped<ICommandHandler<AuthenticateUserCommand, OperationResult<AuthenticatedUserResult>>, AuthenticateUserCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshSessionCommand, OperationResult<AuthenticatedUserResult>>, RefreshSessionCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutUserCommand, OperationResult>, LogoutUserCommandHandler>();
        services.AddScoped<ICommandHandler<ResendActivationCodeCommand, OperationResult>, ResendActivationCodeCommandHandler>();
        services.AddScoped<ICommandHandler<ForgotPasswordCommand, OperationResult>, ForgotPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand, OperationResult>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, OperationResult>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, OperationResult<Guid>>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<InviteUserCommand, OperationResult<Guid>>, InviteUserCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateUserCommand, OperationResult>, ActivateUserCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteInvitationCommand, OperationResult>, CompleteInvitationCommandHandler>();
        services.AddScoped<IQueryHandler<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>, GetPortfolioPositionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>, GetIncomeTaxSummaryQueryHandler>();
        services.AddScoped<IQueryHandler<GetAssetDefinitionsQuery, OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>, GetAssetDefinitionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetStatementEntriesQuery, OperationResult<IReadOnlyCollection<StatementEntryItem>>>, GetStatementEntriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCorporateEventsQuery, OperationResult<IReadOnlyCollection<CorporateEventItem>>>, GetCorporateEventsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, OperationResult<CurrentUserItem>>, GetCurrentUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetAdminUsersQuery, OperationResult<IReadOnlyCollection<AdminUserItem>>>, GetAdminUsersQueryHandler>();
        return services;
    }
}
