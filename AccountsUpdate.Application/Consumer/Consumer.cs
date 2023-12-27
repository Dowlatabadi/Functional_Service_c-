//using MassTransit;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using AccountsUpdate.Application.AccountUpdate.Commands;
using System.Threading.Channels;
using AccountsUpdate.Application.Extensions;

namespace AccountsUpdate.Application.Consumer
{
    public class Consumer : IConsumer<string>
    {
        public Task Consume(ConsumeContext<string> context)
        {
            throw new NotImplementedException();
        }
    }
    public interface IConsumerService : IDisposable
    {
        Task StartConsuming();
    }
    public class RabbitMQConsumerService : IConsumerService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly MediatR.IMediator _mediator;

        public RabbitMQConsumerService(IMediator mediator)
        {
            _mediator = mediator;

            var factory = new ConnectionFactory
            {

                Uri = new Uri("amqp://account-rabbit-mo:4ZamKCspcvikG9@prod-rabbitmq-acc.mofid.dc:32001/Account")
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        public async Task Consume(object? sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received message: {message}");
            var consumeResult = await _mediator.Send(new UpdateAccountRequest(message)).ConfigureAwait(false);
            if (consumeResult) _channel.BasicAck(args.DeliveryTag, multiple: false);
            else _channel.BasicNack(args.DeliveryTag, multiple: false,requeue: true);
        }
        public async Task StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, args) =>
            {
                await Consume(sender, args);
            };

            //10kb
            _channel.BasicQos(0, 10, false);

            _channel.BasicConsume(queue: "AccountChangeQueue_P", autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
