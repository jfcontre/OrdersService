using Microsoft.AspNetCore.Builder;

namespace OrdersService.Api.Middlewares
{
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}