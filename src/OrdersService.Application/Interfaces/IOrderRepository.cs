using System.Threading.Tasks;
using OrdersService.Domain;

namespace OrdersService.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task SaveAsync(Order order);
    }
}