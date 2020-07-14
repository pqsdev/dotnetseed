using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PQS.SeedApp.Business.HostedServices
{
    public class ImportFileBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly SeedAppOptions _config;

        public ImportFileBackgroundService(IServiceProvider services, ILogger<ImportFileBackgroundService> logger, IOptions<SeedAppOptions> config) : base()
        {
            _services = services;
            _logger = logger;
            _config = config.Value;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            


            _logger.LogInformation($"Service Starting...{ JsonSerializer.Serialize(_config.Messaging) }");

            await Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(10000, stoppingToken).Wait();
                    _logger.LogInformation($"Alive");
                }
            }, stoppingToken);

            /*
            ClientConfig algo = new ClientConfig();

            
            var config = new ConsumerConfig(_config.Messaging)
            {
                GroupId = "mobin.notification-group",
                EnableAutoCommit = false,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };



            // Note: If a key or value deserializer is not set (as is the case below), the 
            // deserializer corresponding to the appropriate type from Confluent.Kafka.Deserializers
            // will be used automatically (where available). The default deserializer for string
            // is UTF8. The default deserializer for Ignore returns null for all input data
            // (including non-null data).
            var consumer = new ConsumerBuilder<string, string>(config)
                
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
                    // possibly manually specify start offsets or override the partition assignment provided by
                    // the consumer group by returning a list of topic/partition/offsets to assign to, e.g.:
                    // 
                    // return partitions.Select(tp => new TopicPartitionOffset(tp, externalOffsets[tp]));
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogInformation($"Revoking assignment: [{string.Join(", ", partitions)}]");
                })
                .SetLogHandler((c, message) =>
                {
                    _logger.LogDebug($"[{message.Level}] {message.Message}");
                })
                .Build();

            consumer.Subscribe("mobin-soft");

            _logger.LogInformation($"Service Started...");
            await Task.Run(() => Consume(consumer, stoppingToken));

            */
            _logger.LogInformation("Service Stoped...");
        }

        protected void Consume(IConsumer<string, string> consumer, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        if (consumeResult.IsPartitionEOF)
                        {
                            continue;
                        }
                       
                        Task.Run(() =>
                        {
                            try
                            {
                                Guid processId = Guid.Parse( consumeResult.Message.Value);
                                _logger.LogInformation($"Procesando archivo {consumeResult.Message.Value}");
                                ProcessFile(processId);
                                consumer.Commit(consumeResult);
                                _logger.LogInformation($"Archivo {consumeResult.Message.Value} procesado");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error al procesar archivo {consumeResult.Message.Value}");
                            }
                        }, cancellationToken);




                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogInformation($"Consume error: {e.Error.Reason}");
                    }
                    
                }

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Closing consumer.");
                consumer.Close();
            }
        }

        protected void ProcessFile(Guid processId)
        {
            _logger.LogInformation($"Procesing file {processId}");
            using var scope = _services.CreateScope();
        }
    }



}
