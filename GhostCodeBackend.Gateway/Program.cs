var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.AddDefaultHealthChecks();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.Run();
