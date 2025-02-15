using System;
using System.Text.Json;
using System.Threading.Tasks;
using OrdersService.Application.Interfaces;
using OrdersService.Domain;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace OrdersService.Infrastructure.Messaging
{
    public class SqsQueuePublisher : IQueuePublisher
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _receivedQueueUrl;
        private readonly string _inProcessQueueUrl;
        private readonly string _completedQueueUrl;
        private readonly string _cancelledQueueUrl;

        public SqsQueuePublisher(IAmazonSQS sqsClient,
                                 string receivedQueueUrl,
                                 string inProcessQueueUrl,
                                 string completedQueueUrl,
                                 string cancelledQueueUrl)
        {
            _sqsClient = sqsClient;
            _receivedQueueUrl = receivedQueueUrl;
            _inProcessQueueUrl = inProcessQueueUrl;
            _completedQueueUrl = completedQueueUrl;
            _cancelledQueueUrl = cancelledQueueUrl;
        }

        public async Task PublishAsync(Order order)
        {
            string queueUrl = order.Estado switch
            {
                OrderState.Recibida => _receivedQueueUrl,
                OrderState.EnProceso => _inProcessQueueUrl,
                OrderState.Completada => _completedQueueUrl,
                OrderState.Cancelada => _cancelledQueueUrl,
                _ => throw new ArgumentOutOfRangeException()
            };

            var messageBody = JsonSerializer.Serialize(new
            {
                order.Id,
                order.FechaRegistro,
                order.Descripcion,
                order.FechaEntrega,
                Estado = order.Estado.ToString(),
                order.MotivoCancelacion
            });

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };

            await _sqsClient.SendMessageAsync(sendRequest);
        }
    }
}