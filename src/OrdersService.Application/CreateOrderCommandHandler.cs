using System;
using System.Threading.Tasks;
using OrdersService.Application.Interfaces;
using OrdersService.Domain;

namespace OrdersService.Application.Commands
{
    public class CreateOrderCommandHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IQueuePublisher _queuePublisher;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IQueuePublisher queuePublisher)
        {
            _orderRepository = orderRepository;
            _queuePublisher = queuePublisher;
        }

        public async Task HandleAsync(CreateOrderCommand command)
        {
            // Convertir la propiedad Estado (string) al enum OrderState
            if (!Enum.TryParse<OrderState>(command.Estado, true, out var estado))
            {
                throw new ArgumentException("El estado debe contener alguno de los siguientes valores: Recibida, Completada, Cancelada, EnProceso");
            }

            // Validar fechas
            if (command.FechaEntrega < command.FechaRegistro){
                 throw new ArgumentException("La fecha de entrega no puede ser anterior a la fecha de registro.");
            }       
            
            Order order;
            
            if (string.IsNullOrWhiteSpace(command.MotivoCancelacion))
                {
                    throw new ArgumentException("El motivo de cancelación es obligatorio.");
                }
            else
            {
                order = new Order(command.Id, command.FechaRegistro, command.Descripcion, command.FechaEntrega, estado);
            }

            await _orderRepository.SaveAsync(order);
            await _queuePublisher.PublishAsync(order);
        }
    }
}