using careerhive.api.Middleware;
using careerhive.application;
using careerhive.application.Interfaces.IRepository;
using careerhive.application.Interfaces.IService;
using careerhive.application.Services;
using careerhive.infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using careerhive.api;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddAPI(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CareerHive API v1"));
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");

app.MapControllers();

app.Run();
