using System;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static PokeTrader.Api.BuyOrders.PlaceBuyOrder;

namespace PokeTrader.Api.BuyOrders;

public class ProcessBuyOrder
{
    private readonly ILogger<ProcessBuyOrder> _logger;
    private readonly TableServiceClient _tableService;

    public ProcessBuyOrder(ILogger<ProcessBuyOrder> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableService = tableServiceClient;
    }

    [Function(nameof(ProcessBuyOrder))]
    public async Task Run([QueueTrigger("place-buy-order", Connection = "special")] BuyOrderIssued message)
    {
        _logger.LogInformation($"C# Queue trigger function processed BuyOrderIssued: {message.SellOrderId}");

        try
        {
            var tableClient = _tableService.GetTableClient("buyorders");

            var buyOrder = new BuyOrderEntity
            {
                PartitionKey = "buy_order",
                RowKey = Guid.NewGuid().ToString(),
                SellOrderId = message.SellOrderId.ToString()
            };

            await tableClient.AddEntityAsync(buyOrder);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    public class BuyOrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public string SellOrderId { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
