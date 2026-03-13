var builder = WebApplication.CreateBuilder(args);

builder.AddApiLogging();
builder.Services.AddApiDependencies(builder.Configuration, builder.Environment);

var app = builder.Build();

app.ConfigureApiPipeline();

app.Run();
