using Microsoft.AspNetCore.Http.Features;
using LeaderboardApi;
using LeaderboardApi.Services;
using LeaderboardApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// note(wangjw): Record requestId, traceId for global API Errors handle.
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
        $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection().UseCors(builder =>
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

app.Run();