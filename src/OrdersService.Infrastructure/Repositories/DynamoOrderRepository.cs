using System.Threading.Tasks;
using OrdersService.Application.Interfaces;
using OrdersService.Domain;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace OrdersService.Infrastructure.Repositories
{
    public class DynamoOrderRepository : IOrderRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoOrderRepository(IAmazonDynamoDB dynamoDBClient)
        {
            _context = new DynamoDBContext(dynamoDBClient);
        }

        public async Task SaveAsync(Order order)
        {
            await _context.SaveAsync(order);
        }
    }
}