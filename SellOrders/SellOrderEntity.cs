using Azure;
using Azure.Data.Tables;

namespace PokeTrader.Api.SellOrders;

public class SellOrderEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string PokemonName { get; set; } = string.Empty;
    public string SellPrice { get; set; } = string.Empty;

    public bool IsSold { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}