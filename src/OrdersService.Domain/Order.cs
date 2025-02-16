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
    }
}