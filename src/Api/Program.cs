
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDependencies(builder.Configuration, builder.Environment);

var app = builder.Build();

app.ConfigureApiPipeline();

app.Run();
