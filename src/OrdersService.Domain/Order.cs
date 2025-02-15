using Amazon.DynamoDBv2.DataModel;
using System;

namespace OrdersService.Domain
{
    [DynamoDBTable("OrdersTable")]
    public class Order
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }
        
        [DynamoDBProperty]
        public DateTime FechaRegistro { get; set; }
        
        [DynamoDBProperty]
        public string Descripcion { get; set; }
        
        [DynamoDBProperty]
        public DateTime FechaEntrega { get; set; }
        
        [DynamoDBProperty]
        public OrderState Estado { get; set; }
        
        [DynamoDBProperty]
        public string MotivoCancelacion { get; set; }

        // Constructor parametrizado para uso en la lógica de negocio
        public Order(Guid id, DateTime fechaRegistro, string descripcion, DateTime fechaEntrega, OrderState estado)
        {
            Id = id;
            FechaRegistro = fechaRegistro;
            Descripcion = descripcion;
            FechaEntrega = fechaEntrega;
            Estado = estado;
        }

        // Constructor sin parámetros requerido por DynamoDBContext
        public Order() { }

        // Método de fábrica para órdenes canceladas
        public static Order CrearOrdenCancelada(Guid id, DateTime fechaRegistro, string descripcion, DateTime fechaEntrega, string motivoCancelacion)
        {
            if (string.IsNullOrWhiteSpace(motivoCancelacion))
                throw new ArgumentException("El motivo de cancelación es obligatorio para órdenes canceladas.");

            var order = new Order(id, fechaRegistro, descripcion, fechaEntrega, OrderState.Cancelada);
            order.MotivoCancelacion = motivoCancelacion;
            return order;
        }
    }
}