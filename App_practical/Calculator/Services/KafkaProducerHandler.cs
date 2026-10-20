using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace Calculator.Services
{
    public class KafkaProducerHandler : IDisposable
    {
        private IProducer<byte[], byte[]> kafkaProducer;

        public KafkaProducerHandler(IConfiguration config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:ProducerSettings:BootstrapServers"]
            };

            kafkaProducer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
        }

        public void Dispose()
        {
            kafkaProducer?.Dispose();
        }
    }
}