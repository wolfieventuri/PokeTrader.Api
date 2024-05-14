using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PokeTrader.Api.SellOrders;
using System.Text.Json;
using static PokeTrader.Api.BuyOrders.PlaceBuyOrderFunction;

namespace PokeTrader.Api.Outbox;

public class OutboxFunction
{
    private readonly ILogger<GetSellOrders> _logger;
    private readonly TableServiceClient _tableService;
    private readonly QueueServiceClient _queueService;

    private const string partitionKey = "pokemon_sell_order";
    public OutboxFunction(ILogger<GetSellOrders> logger, TableServiceClient tableService, QueueServiceClient queueService)
    {
        _logger = logger;
        _tableService = tableService;
        _queueService = queueService;
    }

    [Function("OutboxFunction")]
    public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var tableClient = _tableService.GetTableClient(StorageConfiguration.SellOrderTableName);

        var query = tableClient.Query<OutboxEvent>(filter: $"PartitionKey eq '{partitionKey}' and RowKey ge 'outbox_'");

        var queueClient = _queueService.GetQueueClient(StorageConfiguration.OutboxQueueName);

        foreach (var item in query)
        {
            var outboxEvt = JsonSerializer.Deserialize<OutboxEvent>(item.EventData);
            

            var buyOrderIssuedDto = new BuyOrderIssued(Guid.Parse(outboxEvt.RowKey), DateTimeOffset.UtcNow);

            var sendMsgResult = queueClient.SendMessage(Base64Encode(JsonSerializer.Serialize(buyOrderIssuedDto)));

            if (!sendMsgResult.GetRawResponse().IsError)
            {
                _logger.LogInformation($"outbox msg successfully sent for {item.RowKey}");
            }

            var deleteResult = tableClient.DeleteEntity(partitionKey, item.RowKey);

            if (!deleteResult.IsError)
            {
                _logger.LogInformation($"outbox entity deleted after successfully publish {outboxEvt.RowKey}");
            }
        }


        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
}
