using Calculator.Data;
using Calculator.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(
            IConfiguration config,
            IServiceProvider serviceProvider,
            IHttpClientFactory clientFactory)
        {
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:ConsumerSettings:BootstrapServers"],
                GroupId = config["Kafka:ConsumerSettings:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _topic = config["Kafka:TopicName"];
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            try
            {
                _kafkaConsumer.Subscribe(_topic);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подписки на Kafka: {ex.Message}");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(stoppingToken);
                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    if (inputData == null) continue;

                    var operation = inputData.Type_operation;
                    var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, operation);
                    inputData.Result = result.ToString();

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
                    context.DataInputVariants.Add(inputData);
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Ошибка Consume: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки: {ex.Message}");
                }
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}