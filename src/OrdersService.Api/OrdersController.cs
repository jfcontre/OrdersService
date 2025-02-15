using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Commands;
using System;
using System.Threading.Tasks;

namespace OrdersService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CreateOrderCommandHandler _commandHandler;

        public OrdersController(CreateOrderCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            // Sin try/catch: cualquier error será capturado por el middleware global de errores
            if (command.Id == Guid.Empty)
                command.Id = Guid.NewGuid();

            command.FechaRegistro = DateTime.UtcNow;

            await _commandHandler.HandleAsync(command);

            return Accepted(new { command.Id });
        }
    }
}