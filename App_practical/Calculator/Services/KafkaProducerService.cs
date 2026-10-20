using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace Calculator.Services
{
    public class KafkaProducerService<K, V> : IDisposable
    {
        private readonly IProducer<K, V> kafkaHandle;

        public KafkaProducerService(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:ProducerSettings:BootstrapServers"]
            };

            kafkaHandle = new ProducerBuilder<K, V>(producerConfig).Build();
        }

        public Task ProduceAsync(string topic, Message<K, V> message)
            => kafkaHandle.ProduceAsync(topic, message);

        public void Flush(TimeSpan timeout)
            => kafkaHandle.Flush(timeout);

        public void Dispose()
        {
            kafkaHandle.Dispose();
        }
    }
}