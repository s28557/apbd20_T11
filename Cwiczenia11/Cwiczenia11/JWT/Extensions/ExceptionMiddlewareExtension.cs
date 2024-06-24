using System.Net;
using Microsoft.AspNetCore.Diagnostics;

namespace JWT.Extensions;

public static class ExceptionMiddlewareExtension
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var logger = appBuilder.ApplicationServices.GetService(typeof(ILogger<Program>)) as ILogger;
                    logger.LogError($"Something went wrong: {contextFeature.Error}");

                    await context.Response.WriteAsync(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Internal Server Error. Please try again later."
                    }.ToString());
                }
            });
        });
    }
}