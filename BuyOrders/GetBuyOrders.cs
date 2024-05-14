using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PokeTrader.Api.SellOrders;
using static PokeTrader.Api.BuyOrders.ProcessBuyOrder;
using static PokeTrader.Api.StorageConfiguration;

namespace PokeTrader.Api.BuyOrders;

public class GetBuyOrders
{
    private readonly ILogger<GetBuyOrders> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public GetBuyOrders(ILogger<GetBuyOrders> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function("GetBuyOrders")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var tableClient = _tableServiceClient.GetTableClient(BuyOrderTableName);

        Page<BuyOrderEntity>? page = tableClient
            .Query<BuyOrderEntity>(filter: $"PartitionKey eq 'buy_order'")
            .AsPages(pageSizeHint: 20)
            .FirstOrDefault();

        var sellOrders = new List<BuyOrderEntity>();

        if (page != null)
        {
            foreach (var item in page.Values)
            {
                sellOrders.Add(item);
            }
        }
        return new OkObjectResult(sellOrders);
    }
}
