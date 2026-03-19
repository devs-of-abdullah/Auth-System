using Microsoft.AspNetCore.Diagnostics;

public static class ExceptionMExtensions
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                var (statusCode, message) = exception switch
                {
                    KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                    UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, exception.Message),
                    InvalidOperationException => (StatusCodes.Status409Conflict, exception.Message),
                    ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
                };

                if (statusCode == StatusCodes.Status500InternalServerError && exception != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "An unhandled exception has occurred while executing the request.");
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode,
                    message
                });
            });
        });
    }
}