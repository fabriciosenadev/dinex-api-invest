
namespace DinExApi.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDispatcher, ApplicationDispatcher>();
        services.AddScoped<ICommandHandler<RegisterMovementCommand, OperationResult<Guid>>, RegisterMovementCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterStatementEntryCommand, OperationResult<Guid>>, RegisterStatementEntryCommandHandler>();
        services.AddScoped<ICommandHandler<AuthenticateUserCommand, OperationResult<AuthenticatedUserResult>>, AuthenticateUserCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshSessionCommand, OperationResult<AuthenticatedUserResult>>, RefreshSessionCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutUserCommand, OperationResult>, LogoutUserCommandHandler>();
        services.AddScoped<ICommandHandler<ResendActivationCodeCommand, OperationResult>, ResendActivationCodeCommandHandler>();
        services.AddScoped<ICommandHandler<ForgotPasswordCommand, OperationResult>, ForgotPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand, OperationResult>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, OperationResult>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, OperationResult<Guid>>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateUserCommand, OperationResult>, ActivateUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>, GetPortfolioPositionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetStatementEntriesQuery, OperationResult<IReadOnlyCollection<StatementEntryItem>>>, GetStatementEntriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, OperationResult<CurrentUserItem>>, GetCurrentUserQueryHandler>();
        return services;
    }
}
