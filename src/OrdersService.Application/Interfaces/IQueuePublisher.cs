using System.Threading.Tasks;
using OrdersService.Domain;

namespace OrdersService.Application.Interfaces
{
    public interface IQueuePublisher
    {
        Task PublishAsync(Order order);
    }
}