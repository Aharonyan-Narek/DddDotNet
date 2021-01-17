﻿using DddDotNet.Domain.Infrastructure.MessageBrokers;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DddDotNet.Infrastructure.MessageBrokers.RabbitMQ
{
    public class RabbitMQSender<T> : IMessageSender<T>
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public RabbitMQSender(RabbitMQSenderOptions options)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = options.HostName,
                UserName = options.UserName,
                Password = options.Password,
            };

            _exchangeName = options.ExchangeName;
            _routingKey = options.RoutingKey;
        }

        public void Send(T message, MetaData metaData = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message<T>
                {
                    Data = message,
                    MetaData = metaData,
                }));
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: _exchangeName,
                                     routingKey: _routingKey,
                                     basicProperties: properties,
                                     body: body);
            }
        }

        public Task SendAsync(T message, MetaData metaData = null, CancellationToken cancellationToken = default)
        {
            Send(message, metaData);
            return Task.CompletedTask;
        }
    }
}
