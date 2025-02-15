using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrdersService.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Procesa la siguiente parte del pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Registra el error
                _logger.LogError(ex, "Ocurrió un error no controlado");
                // Maneja la excepción y envía una respuesta estandarizada
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Define el status code según el tipo de error
            var code = HttpStatusCode.InternalServerError; // Por defecto: 500

            // Ejemplo: si es un error de validación o ArgumentException, se puede devolver 400.
            if (exception is ArgumentException)
            {
                code = HttpStatusCode.BadRequest;
            }

            // Estructura estandarizada de la respuesta de error
            var errorResponse = new
            {
                error = new
                {
                    code = code.ToString(),
                    message = exception.Message,
                    details = exception.InnerException?.Message
                }
            };

            var result = JsonSerializer.Serialize(errorResponse);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}