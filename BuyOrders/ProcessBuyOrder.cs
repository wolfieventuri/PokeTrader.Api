using System;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static PokeTrader.Api.BuyOrders.PlaceBuyOrderFunction;

namespace PokeTrader.Api.BuyOrders;

public partial class ProcessBuyOrder
{
    private readonly ILogger<ProcessBuyOrder> _logger;
    private readonly TableServiceClient _tableService;

    public ProcessBuyOrder(ILogger<ProcessBuyOrder> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableService = tableServiceClient;
    }


    [Function(nameof(ProcessBuyOrder))]
    public async Task Run([QueueTrigger(StorageConfiguration.OutboxQueueName, Connection = "special")] QueueMessage message)
    {

        var dto = JsonSerializer.Deserialize<BuyOrderIssued>(message.Body);
        _logger.LogInformation($"C# Queue trigger function processed BuyOrderIssued: {dto.SellOrderId}");

        try
        {
            var tableClient = _tableService.GetTableClient(StorageConfiguration.BuyOrderTableName);

            var buyOrder = new BuyOrderEntity
            {
                PartitionKey = "buy_order",
                RowKey = Guid.NewGuid().ToString(),
                SellOrderId = dto.SellOrderId.ToString()
            };

            var res = await tableClient.AddEntityAsync(buyOrder);

            _logger.LogDebug($"response of adding buy order: {res.Status}");
            _logger.LogDebug("response of adding buy order {res}", res);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}
