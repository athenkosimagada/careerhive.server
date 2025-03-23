using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using careerhive.application.Interfaces.IRepository;
using careerhive.domain.Entities;
using careerhive.infrastructure.Data;
using careerhive.infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace careerhive.api;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient", policy =>
            {
                policy.WithOrigins("https://localhost:7001", "https://localhost:7000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CareerHive API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Paste only the <access-token> bellow."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var error = context.ModelState.Values
                             .SelectMany(v => v.Errors)
                             .FirstOrDefault();

                var errorMessage = error?.ErrorMessage ?? "Unknown error has occurred.";
                return new BadRequestObjectResult(new
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Error = errorMessage
                });
            };

        });

        services.AddRateLimiter(options =>
        {
            /*options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                 RateLimitPartition.GetFixedWindowLimiter(
                     partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                     factory: partition => new FixedWindowRateLimiterOptions
                     {
                         AutoReplenishment = true,
                         PermitLimit = 100,
                         QueueLimit = 0,
                         Window = TimeSpan.FromMinutes(1)
                     }));*/

            options.AddPolicy("login", httpContext =>
                 RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress,
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(3)
                    }));

            options.AddPolicy("register", httpContext =>
                 RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("get", httpContext =>
            {
                bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: isAuthenticated
                        ? httpContext.User.Identity?.Name
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = isAuthenticated ? 100 : 50,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("post", httpContext =>
            {
                bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: isAuthenticated
                        ? httpContext.User.Identity?.Name
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = isAuthenticated ? 10 : 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("put", httpContext =>
            {
                bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: isAuthenticated
                        ? httpContext.User.Identity?.Name
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = isAuthenticated ? 10 : 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("delete", httpContext =>
            {
                bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: isAuthenticated
                        ? httpContext.User.Identity?.Name
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = isAuthenticated ? 10 : 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                var response = context.HttpContext.Response;
                response.ContentType = "application/json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var errorResponse = new
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        Message = $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
                                  $"Read more about our rate limits at https://example.org/docs/ratelimiting."
                    };
                    await response.WriteAsJsonAsync(errorResponse, cancellationToken);
                }
                else
                {
                    var errorResponse = new
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        Message = "Too many requests. Please try again later. " +
                                  $"Read more about our rate limits at https://example.org/docs/ratelimiting."
                    };
                    await response.WriteAsJsonAsync(errorResponse, cancellationToken);
                }
            };
        });

        return services;
    }
}
