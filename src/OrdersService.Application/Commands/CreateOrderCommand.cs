using System;
using OrdersService.Domain;

namespace OrdersService.Application.Commands
{
    public class CreateOrderCommand
    {
        public Guid Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaEntrega { get; set; }
        public String Estado { get; set; }
        public string MotivoCancelacion { get; set; }
    }
}