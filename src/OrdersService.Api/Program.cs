using Amazon;
using Amazon.Runtime;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Application.Commands;
using OrdersService.Application.Interfaces;
using OrdersService.Infrastructure.Messaging;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Infrastructure; // Para DynamoDBInitializer
using Amazon.DynamoDBv2;
using Amazon.SQS;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using OrdersService.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Configurar AWSOptions para DynamoDB Local
var dynamoAwsOptions = new AWSOptions
{
    Region = RegionEndpoint.USEast1,
    DefaultClientConfig = { ServiceURL = builder.Configuration["DynamoDB:ServiceURL"] },
    Credentials = new BasicAWSCredentials("test", "test")
};
builder.Services.AddDefaultAWSOptions(dynamoAwsOptions); // Usado para DynamoDB
builder.Services.AddAWSService<IAmazonDynamoDB>(dynamoAwsOptions);

// Configurar AWSOptions para SQS (LocalStack)
var sqsAwsOptions = new AWSOptions
{
    Region = RegionEndpoint.USEast1,
    DefaultClientConfig = { ServiceURL = builder.Configuration["SQS:ServiceURL"] },
    Credentials = new BasicAWSCredentials("test", "test")
};
//builder.Services.AddAWSService<IAmazonSQS>(sqsAwsOptions);
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    // Crear opciones específicas para SQS
    var options = new AWSOptions
    {
        Region = RegionEndpoint.USEast1,
        DefaultClientConfig = { ServiceURL = builder.Configuration["SQS:ServiceURL"] },
        Credentials = new BasicAWSCredentials("test", "test")
    };
    return options.CreateServiceClient<IAmazonSQS>();
});
// Configurar controladores y conversor de enums para JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Registrar repositorios y handlers
builder.Services.AddScoped<IOrderRepository, DynamoOrderRepository>();
builder.Services.AddScoped<IQueuePublisher>(sp =>
    new SqsQueuePublisher(
        sp.GetRequiredService<IAmazonSQS>(),
        builder.Configuration["SQS:Queues:ReceivedQueueUrl"],
        builder.Configuration["SQS:Queues:InProcessQueueUrl"],
        builder.Configuration["SQS:Queues:CompletedQueueUrl"],
        builder.Configuration["SQS:Queues:CancelledQueueUrl"]
    ));
builder.Services.AddScoped<CreateOrderCommandHandler>();

// Registra servicios para explorar endpoints y generar la documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrdersService API",
        Version = "v1",
        Description = "API para la gestión de órdenes de servicio"
    });

   
});

var app = builder.Build();

// Inicializar la tabla en DynamoDB Local
using (var scope = app.Services.CreateScope())
{
    var dynamoDbClient = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    await OrdersService.Infrastructure.DynamoDBInitializer.EnsureTableExistsAsync(dynamoDbClient, "OrdersTable");
}

app.UseGlobalErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersService API V1");
    });
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();