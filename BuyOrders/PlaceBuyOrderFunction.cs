using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PokeTrader.Api.SellOrders;
using System.Text.Json;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;


namespace PokeTrader.Api.BuyOrders
{
    public class PlaceBuyOrderFunction
    {
        private readonly ILogger<PlaceBuyOrderFunction> _logger;
        private readonly TableServiceClient _tableServiceClient;


        public PlaceBuyOrderFunction(ILogger<PlaceBuyOrderFunction> logger, TableServiceClient tableServiceClient)
        {
            _logger = logger;
            _tableServiceClient = tableServiceClient;
        }

        [Function(nameof(PlaceBuyOrderFunction))]
        public async Task<BuyOrderIssued> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [FromBody] BuyOrderDto dto)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var tableClient = _tableServiceClient.GetTableClient(StorageConfiguration.SellOrderTableName);


            var sellOrder = await tableClient.GetEntityAsync<SellOrderEntity>("pokemon_sell_order", dto.SellOrderId.ToString());
                
            if (sellOrder != null)
            {

                List<TableTransactionAction> actions = new();


                // add actions
                sellOrder.Value.IsSold = true;

                actions.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge, sellOrder.Value));
                actions.Add(new TableTransactionAction(TableTransactionActionType.Add, new OutboxEvent
                {
                    PartitionKey = "pokemon_sell_order",
                    RowKey = $"outbox_{dto.SellOrderId}",
                    EventName = "BuyOrderPlaced",
                    EventData = JsonSerializer.Serialize(sellOrder.Value)
                })) ;


                var transactionResponse = await tableClient.SubmitTransactionAsync(actions);
                // submit request
                return new BuyOrderIssued(dto.SellOrderId, DateTimeOffset.UtcNow);
            }

            return new BuyOrderIssued(dto.SellOrderId, DateTimeOffset.UtcNow);

        }

        public class OutboxEvent : ITableEntity
        {
            public string PartitionKey { get; set; } = string.Empty;
            public string RowKey { get; set; } = string.Empty;
            public string EventName { get; set; } = string.Empty;
            public string EventData { get; set; } = string.Empty;

            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }

        public record BuyOrderDto(Guid SellOrderId);
        public record BuyOrderIssued(Guid SellOrderId, DateTimeOffset DateIssued);
    }
}
