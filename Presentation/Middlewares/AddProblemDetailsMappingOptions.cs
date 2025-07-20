using Common.Exceptions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Middlewares
{
    public static class ProblemDetailsMappingOptions
    {
        public static IServiceCollection AddProblemDetailsMappingOptions(this IServiceCollection services)
        {
            Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(services, options =>
            {
                options.OnBeforeWriteDetails = (ctx, problem) =>
                {
                    problem.Extensions["traceId"] = ctx.TraceIdentifier;
                    problem.Instance = ctx.Request.Path;
                };


                //This will map Unauthorized exceptions
                options.Map<UnauthorizedException>((_, exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail,
                    };
                });

                // This will map validation errors.
                options.Map<FluentValidation.ValidationException>((_, exception) =>
                {
                    var errors = exception.Errors.GroupBy(failure => failure.PropertyName)
                        .Select(failures => failures)
                        .ToDictionary(failures => failures.Key,
                            failures => failures.Select(failure => failure.ErrorMessage).ToArray());

                    return new ValidationProblemDetails(errors)
                    {
                        Type = HttpCodeTypes.Error400Type,
                        Status = StatusCodes.Status400BadRequest
                    };
                });

                // This will map service errors.
                #region Service exception
                options.Map<ForbiddenException>((exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail
                    };
                });

                options.Map<NotFoundException>((exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail
                    };
                });

                options.Map<ConflictException>((exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail
                    };
                });
                options.Map<BadRequestException>((exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail
                    };
                });

                options.Map<GoneException>((exception) =>
                {
                    return new ProblemDetails
                    {
                        Type = exception.Type,
                        Title = exception.Title,
                        Status = exception.Status,
                        Detail = exception.Detail
                    };
                });
                #endregion


                options.Map<Exception>((ctx, exception) =>
                {
                    return new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError)
                    {
                        Type = HttpCodeTypes.Error500Type,

                    };
                });

                options.ShouldLogUnhandledException = (ctx, exception, problem) =>
                {
                    if (problem.Status is >= 500)
                    {
                        var logger = services.BuildServiceProvider().GetService<ILogger<ProblemDetails>>();
                        var messageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} ";
                        using (LogContext.PushProperty("StatusCode", problem.Status))
                        using (LogContext.PushProperty("RequestPath", ctx.Request.Path))
                        using (LogContext.PushProperty("RequestMethod", ctx.Request.Method))
                        using (LogContext.PushProperty("ContentType", ctx.Response.ContentType))
                        using (LogContext.PushProperty("Scheme", ctx.Request.Scheme))
                        using (LogContext.PushProperty("Protocol", ctx.Request.Protocol))
                        using (LogContext.PushProperty("Host", ctx.Request.Host))
                        {
                            logger.LogError(exception, messageTemplate);
                        }
                    }
                    return false;
                };
            });

            return services;
        }
    }
}