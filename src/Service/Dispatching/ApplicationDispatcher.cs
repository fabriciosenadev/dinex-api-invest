namespace DinExApi.Service;

public sealed class ApplicationDispatcher(
    IServiceProvider serviceProvider,
    ILogger<ApplicationDispatcher>? logger = null) : IApplicationDispatcher
{
    public async Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var currentLogger = logger ?? NullLogger<ApplicationDispatcher>.Instance;
        var commandName = typeof(TCommand).Name;
        var startedAt = DateTime.UtcNow;
        var handler = serviceProvider.GetService(typeof(ICommandHandler<TCommand, TResponse>)) as ICommandHandler<TCommand, TResponse>
            ?? throw new InvalidOperationException($"Command handler not found for {commandName}.");

        currentLogger.LogInformation("Handling command {CommandName}.", commandName);
        currentLogger.LogDebug("Command payload {CommandName}: {Payload}", commandName, SerializePayload(command));

        try
        {
            var response = await handler.HandleAsync(command, cancellationToken);
            LogResult(currentLogger, commandName, response, startedAt, isQuery: false);
            return response;
        }
        catch (Exception exception)
        {
            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
            currentLogger.LogError(exception, "Command {CommandName} failed after {ElapsedMs:0} ms.", commandName, elapsedMs);
            throw;
        }
    }

    public async Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var currentLogger = logger ?? NullLogger<ApplicationDispatcher>.Instance;
        var queryName = typeof(TQuery).Name;
        var startedAt = DateTime.UtcNow;
        var handler = serviceProvider.GetService(typeof(IQueryHandler<TQuery, TResponse>)) as IQueryHandler<TQuery, TResponse>
            ?? throw new InvalidOperationException($"Query handler not found for {queryName}.");

        currentLogger.LogInformation("Handling query {QueryName}.", queryName);
        currentLogger.LogDebug("Query payload {QueryName}: {Payload}", queryName, SerializePayload(query));

        try
        {
            var response = await handler.HandleAsync(query, cancellationToken);
            LogResult(currentLogger, queryName, response, startedAt, isQuery: true);
            return response;
        }
        catch (Exception exception)
        {
            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
            currentLogger.LogError(exception, "Query {QueryName} failed after {ElapsedMs:0} ms.", queryName, elapsedMs);
            throw;
        }
    }

    private static void LogResult<TResponse>(ILogger currentLogger, string operationName, TResponse response, DateTime startedAt, bool isQuery)
    {
        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
        var operationType = isQuery ? "Query" : "Command";

        if (response is OperationResultBase operationResult)
        {
            if (operationResult.Succeeded)
            {
                currentLogger.LogInformation(
                    "{OperationType} {OperationName} completed successfully in {ElapsedMs:0} ms.",
                    operationType,
                    operationName,
                    elapsedMs);
                return;
            }

            currentLogger.LogWarning(
                "{OperationType} {OperationName} completed with errors in {ElapsedMs:0} ms. NotFound={IsNotFound}, InternalServerError={InternalServerError}, ErrorCount={ErrorCount}.",
                operationType,
                operationName,
                elapsedMs,
                operationResult.IsNotFound,
                operationResult.InternalServerError,
                operationResult.Errors.Count);
            currentLogger.LogDebug(
                "{OperationType} {OperationName} errors: {Errors}.",
                operationType,
                operationName,
                operationResult.Errors.Take(10).ToArray());

            return;
        }

        currentLogger.LogInformation(
            "{OperationType} {OperationName} completed in {ElapsedMs:0} ms.",
            operationType,
            operationName,
            elapsedMs);
    }

    private static string SerializePayload<TPayload>(TPayload payload)
    {
        try
        {
            return JsonSerializer.Serialize(payload);
        }
        catch
        {
            return "<serialization-failed>";
        }
    }
}
