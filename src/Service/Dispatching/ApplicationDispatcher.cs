
namespace DinExApi.Service;

public sealed class ApplicationDispatcher(IServiceProvider serviceProvider) : IApplicationDispatcher
{
    public async Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetService(typeof(ICommandHandler<TCommand, TResponse>)) as ICommandHandler<TCommand, TResponse>
            ?? throw new InvalidOperationException($"Command handler not found for {typeof(TCommand).Name}.");

        return await handler.HandleAsync(command, cancellationToken);
    }

    public async Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetService(typeof(IQueryHandler<TQuery, TResponse>)) as IQueryHandler<TQuery, TResponse>
            ?? throw new InvalidOperationException($"Query handler not found for {typeof(TQuery).Name}.");

        return await handler.HandleAsync(query, cancellationToken);
    }
}
