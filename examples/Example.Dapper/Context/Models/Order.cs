namespace ClickHouse.Facades.Example;

public class Order
{
	public uint UserId { get; set; }
	public ulong OrderId { get; set; }
	public DateTime DateTimeUtc { get; set; }
	public decimal Price { get; set; }
}
