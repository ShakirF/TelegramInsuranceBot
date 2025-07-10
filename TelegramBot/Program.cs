using Telegram.Bot;
using Persistence;
using Application;
using Infrastructure.Telegram.Service;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError($"{contextFeature.Error}");

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                error = "? An unexpected error occurred. Please try again later."
            }));
        }
    });
});
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unhandled error: {ex.Message}");
        throw;
    }
});
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}
app.MapControllers();
app.Run();
