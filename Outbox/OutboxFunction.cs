using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PokeTrader.Api.SellOrders;
using static PokeTrader.Api.BuyOrders.PlaceBuyOrder;

namespace PokeTrader.Api.Outbox;

//public class OutboxFunction
//{
//    private readonly ILogger<GetSellOrders> _logger;
//    private readonly TableServiceClient _tableService;

//    private const string partitionKey = "pokemon_sell_order";
//    public OutboxFunction(ILogger<GetSellOrders> logger, TableServiceClient tableService)
//    {
//        _logger = logger;
//        _tableService = tableService;
//    }

//    [Function("OutboxFunction")]
//    public void Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer)
//    {
//        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

//        var tableClient = _tableService.GetTableClient("sellorders");

//        var query = tableClient.Query<OutboxEvent>(filter: $"PartitionKey eq '{partitionKey}' and RowKey ge 'outbox_'");

//        var queueService = new QueueServiceClient("AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;");
//        var queueClient = queueService.GetQueueClient("outbox-bus");

//        foreach (var item in query)
//        {
//            var outboxEvt = item;

//            var sendMsgResult = queueClient.SendMessage($"processed {outboxEvt.RowKey}");

//            if (!sendMsgResult.GetRawResponse().IsError)
//            {
//                _logger.LogInformation($"outbox msg successfully sent for {outboxEvt.RowKey}");
//            }

//            var deleteResult = tableClient.DeleteEntity(partitionKey, outboxEvt.RowKey);

//            if (!deleteResult.IsError)
//            {
//                _logger.LogInformation($"outbox entity deleted after successfully publish {outboxEvt.RowKey}");
//            }
//        }

        
//        if (myTimer.ScheduleStatus is not null)
//        {
//            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
//        }
//    }
//}
