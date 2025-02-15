using System;
using System.Threading.Tasks;
using Moq;
using OrdersService.Application.Commands;
using OrdersService.Application.Interfaces;
using OrdersService.Domain;
using Xunit;

namespace OrdersService.Tests
{
    public class CreateOrderCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ValidCommand_ShouldStoreAndPublishOrder()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var queuePublisherMock = new Mock<IQueuePublisher>();

            // Crear instancia del handler con dependencias mockeadas
            var handler = new CreateOrderCommandHandler(orderRepositoryMock.Object, queuePublisherMock.Object);

            // Definir un comando válido (para una orden no cancelada)
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Reparación de pantalla",
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = OrderState.Recibida
            };

            // Act
            await handler.HandleAsync(command);

            // Assert: Verificamos que se haya guardado la orden y se haya publicado en la cola.
            orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
            queuePublisherMock.Verify(q => q.PublishAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_CancelledOrderWithoutMotivo_ShouldThrowException()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var queuePublisherMock = new Mock<IQueuePublisher>();

            var handler = new CreateOrderCommandHandler(orderRepositoryMock.Object, queuePublisherMock.Object);

            // Definir un comando para una orden cancelada, pero sin proporcionar motivo
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Orden Cancelada sin motivo",
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = OrderState.Cancelada,
                MotivoCancelacion = "" // Vacío, debería provocar error
            };

            // Act & Assert: Se espera que se lance una excepción debido a la falta de motivo.
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
        }
    }
}