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
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IQueuePublisher> _queuePublisherMock;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _queuePublisherMock = new Mock<IQueuePublisher>();
            _handler = new CreateOrderCommandHandler(_orderRepositoryMock.Object, _queuePublisherMock.Object);
        }

        /// <summary>
        /// ✅ **Prueba de caso válido:** Debe almacenar y publicar la orden correctamente.
        /// </summary>
        [Fact]
        public async Task HandleAsync_ValidCommand_ShouldStoreAndPublishOrder()
        {
            // Arrange: Crear un comando válido con estado Recibida.
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Reparación de pantalla",
                FechaRegistro = DateTime.UtcNow,
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = OrderState.Recibida.ToString(), // Estado correcto como string.
                MotivoCancelacion = "Test"
            };

            // Act
            await _handler.HandleAsync(command);

            // Assert: Se verifica que se haya almacenado la orden y publicado en la cola.
            _orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
            _queuePublisherMock.Verify(q => q.PublishAsync(It.IsAny<Order>()), Times.Once);
        }

        /// <summary>
        /// ❌ **Prueba de validación:** Debe lanzar una excepción si el estado es inválido.
        /// </summary>
        [Fact]
        public async Task HandleAsync_InvalidEstado_ShouldThrowException()
        {
            // Arrange: Crear un comando con estado no válido.
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Orden con estado inválido",
                FechaRegistro = DateTime.UtcNow,
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = "EstadoInvalido" // Estado incorrecto
            };

            // Act & Assert: Se espera una excepción con el mensaje adecuado.
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
            Assert.Equal("El estado debe contener alguno de los siguientes valores: Recibida, Completada, Cancelada, EnProceso", exception.Message);
        }

        /// <summary>
        /// ❌ **Prueba de validación:** Debe lanzar una excepción si la orden cancelada no tiene un motivo de cancelación.
        /// </summary>
        [Fact]
        public async Task HandleAsync_CancelledOrderWithoutMotivo_ShouldThrowException()
        {
            // Arrange: Crear una orden Cancelada sin motivo.
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Orden Cancelada sin motivo",
                FechaRegistro = DateTime.UtcNow,
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = OrderState.Cancelada.ToString(),
                MotivoCancelacion = "" // ❌ Campo vacío, debería fallar.
            };

            // Act & Assert: Se espera que lance una excepción con el mensaje correcto.
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
            Assert.Equal("El motivo de cancelación es obligatorio para órdenes canceladas.", exception.Message);
        }

        /// <summary>
        /// ❌ **Prueba de validación:** Debe lanzar una excepción si la fecha de entrega es menor a la fecha de registro.
        /// </summary>
        [Fact]
        public async Task HandleAsync_FechaEntregaMenorAFechaRegistro_ShouldThrowException()
        {
            // Arrange: Crear una orden con fecha de entrega menor a la fecha de registro.
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Orden con fecha de entrega inválida",
                FechaRegistro = DateTime.UtcNow,
                FechaEntrega = DateTime.UtcNow.AddDays(-1), // ❌ Fecha de entrega en el pasado.
                Estado = OrderState.Recibida.ToString()
            };

            // Act & Assert: Se espera que lance una excepción con el mensaje adecuado.
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command));
            Assert.Equal("La fecha de entrega no puede ser anterior a la fecha de registro.", exception.Message);
        }

        /// <summary>
        /// ✅ **Prueba de caso válido:** Debe permitir crear una orden cancelada con motivo de cancelación.
        /// </summary>
        [Fact]
        public async Task HandleAsync_CancelledOrderWithMotivo_ShouldStoreAndPublishOrder()
        {
            // Arrange: Crear una orden Cancelada con motivo válido.
            var command = new CreateOrderCommand
            {
                Id = Guid.NewGuid(),
                Descripcion = "Orden Cancelada con motivo",
                FechaRegistro = DateTime.UtcNow,
                FechaEntrega = DateTime.UtcNow.AddDays(1),
                Estado = OrderState.Cancelada.ToString(),
                MotivoCancelacion = "Cliente canceló la orden" // ✅ Motivo válido
            };

            // Act
            await _handler.HandleAsync(command);

            // Assert: Se verifica que se haya almacenado y publicado la orden correctamente.
            _orderRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
            _queuePublisherMock.Verify(q => q.PublishAsync(It.IsAny<Order>()), Times.Once);
        }
    }
}