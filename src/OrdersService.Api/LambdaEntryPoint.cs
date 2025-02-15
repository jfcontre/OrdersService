using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OrdersService.Api
{
    public class LambdaEntryPoint : APIGatewayHttpApiV2ProxyFunction
    {
        protected override IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Aquí usamos el Program.cs configurado para minimal API
                    webBuilder.UseStartup<Program>(); // Esto requeriría que Program sea una clase Startup alternativa
                });
    }
}