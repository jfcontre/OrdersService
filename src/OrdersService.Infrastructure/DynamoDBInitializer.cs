using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrdersService.Infrastructure
{
    public static class DynamoDBInitializer
    {
        public static async Task EnsureTableExistsAsync(IAmazonDynamoDB dynamoDbClient, string tableName)
        {
            try
            {
                // Intenta describir la tabla
                var response = await dynamoDbClient.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
                // Si se describe, la tabla ya existe.
                System.Console.WriteLine($"La tabla {tableName} ya existe.");
            }
            catch (ResourceNotFoundException)
            {
                // La tabla no existe, se procede a crearla.
                System.Console.WriteLine($"La tabla {tableName} no existe. Creándola...");
                var createRequest = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition { AttributeName = "Id", AttributeType = "S" }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement { AttributeName = "Id", KeyType = "HASH" }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 1,
                        WriteCapacityUnits = 1
                    }
                };

                var createResponse = await dynamoDbClient.CreateTableAsync(createRequest);
                System.Console.WriteLine($"Tabla {tableName} creada. Estado: {createResponse.TableDescription.TableStatus}");
            }
        }
    }
}