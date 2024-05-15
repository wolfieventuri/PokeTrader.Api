using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using PokeTrader.Api;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddAzureClients(clients =>
        {
            if (context.HostingEnvironment.IsDevelopment()) {
                var conString = context.Configuration.GetValue<string>("special");
                clients.AddTableServiceClient(connectionString: conString);
                clients.AddQueueServiceClient(connectionString: conString);
                clients.AddBlobServiceClient(connectionString: conString);
                var tableServiceClient = new TableServiceClient(conString);
                var queueServiceClient = new QueueServiceClient(conString);
                var blobServiceClient = new BlobServiceClient(conString);
                tableServiceClient.CreateTableIfNotExists(StorageConfiguration.SellOrderTableName);
                tableServiceClient.CreateTableIfNotExists(StorageConfiguration.BuyOrderTableName);
                queueServiceClient.CreateQueue(StorageConfiguration.OutboxQueueName);
                queueServiceClient.CreateQueue(StorageConfiguration.OutboxQueueName);

            }
            else
            {
                var tableServiceUri = context.Configuration.GetValue<string>("special:tableServiceUri");
                clients.AddTableServiceClient(serviceUri: new Uri(tableServiceUri));
                clients.UseCredential(new DefaultAzureCredential());
            }
        });

        services.AddOpenTelemetry()
        .WithMetrics(x =>
        {
            x.AddRuntimeInstrumentation()
            .AddMeter(
                "Microsoft.AspNetCore.Hosting",
                "System.Net.Http",
                "PokeTrader.Api"
                );
        })
        .WithTracing(x =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                x.SetSampler<AlwaysOnSampler>();
            }

            x.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });

        services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        services.ConfigureOpenTelemetryMeterProvider(logging => logging.AddOtlpExporter());
        services.ConfigureOpenTelemetryTracerProvider(logging => logging.AddOtlpExporter());
        services.AddMetrics();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddOpenTelemetry(x =>
        {
            x.IncludeScopes = true;
            x.IncludeFormattedMessage = true;
        });
    })
    .Build();



host.Run();