using Azure;
using Azure.Data.Tables;

namespace PokeTrader.Api.BuyOrders;

public partial class ProcessBuyOrder
{
    public class BuyOrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public string SellOrderId { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
