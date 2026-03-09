using Microsoft.Extensions.DependencyInjection;

namespace DinExApi.Tests;

public sealed class ApplicationDispatcherTests
{
    [Fact]
    public async Task SendAsync_Should_Call_Command_Handler()
    {
        var expected = new OperationResult<string>().SetData("ok");
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<TestCommand, OperationResult<string>>>(new TestCommandHandler(expected));
        services.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IApplicationDispatcher>();

        var response = await dispatcher.SendAsync<TestCommand, OperationResult<string>>(new TestCommand("value"));

        Assert.True(response.Succeeded);
        Assert.Equal("ok", response.Data);
    }

    [Fact]
    public async Task QueryAsync_Should_Call_Query_Handler()
    {
        var expected = new OperationResult<string>().SetData("query-ok");
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<TestQuery, OperationResult<string>>>(new TestQueryHandler(expected));
        services.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IApplicationDispatcher>();

        var response = await dispatcher.QueryAsync<TestQuery, OperationResult<string>>(new TestQuery("value"));

        Assert.True(response.Succeeded);
        Assert.Equal("query-ok", response.Data);
    }

    [Fact]
    public async Task SendAsync_Should_Throw_When_Handler_Not_Registered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IApplicationDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.SendAsync<TestCommand, OperationResult<string>>(new TestCommand("value")));
    }

    private sealed record TestCommand(string Value) : ICommand<OperationResult<string>>;
    private sealed record TestQuery(string Value) : IQuery<OperationResult<string>>;

    private sealed class TestCommandHandler(OperationResult<string> result)
        : ICommandHandler<TestCommand, OperationResult<string>>
    {
        public Task<OperationResult<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    private sealed class TestQueryHandler(OperationResult<string> result)
        : IQueryHandler<TestQuery, OperationResult<string>>
    {
        public Task<OperationResult<string>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
