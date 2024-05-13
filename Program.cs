using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddAzureClients(clients =>
        {
            if (context.HostingEnvironment.IsDevelopment()) {
                var conString = context.Configuration.GetValue<string>("special:tableServiceUri");
                clients.AddTableServiceClient(connectionString: conString);
            }
            else {
                clients.AddTableServiceClient(serviceUri: new Uri(context.Configuration.GetValue<string>("special:tableServiceUri")));
                clients.UseCredential(new DefaultAzureCredential());
            }
        });

        //services.AddOpenTelemetry()
        //.WithMetrics(x =>
        //{
        //    x.AddRuntimeInstrumentation()
        //    .AddMeter(
        //        "Microsoft.AspNetCore.Hosting",
        //        "System.Net.Http",
        //        "PokeTrader.Api"
        //        );
        //})
        //.WithTracing(x =>
        //{
        //    if (context.HostingEnvironment.IsDevelopment())
        //    {
        //        x.SetSampler<AlwaysOnSampler>();
        //    }

        //    x.AddAspNetCoreInstrumentation()
        //        .AddHttpClientInstrumentation();
        //});

        //services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        //services.ConfigureOpenTelemetryMeterProvider(logging => logging.AddOtlpExporter());
        //services.ConfigureOpenTelemetryTracerProvider(logging => logging.AddOtlpExporter());
        //services.AddMetrics();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    //.ConfigureLogging(logging =>
    //{
    //    logging.AddOpenTelemetry(x =>
    //    {
    //        x.IncludeScopes = true;
    //        x.IncludeFormattedMessage = true;
    //    });
    //})
    .Build();



host.Run();