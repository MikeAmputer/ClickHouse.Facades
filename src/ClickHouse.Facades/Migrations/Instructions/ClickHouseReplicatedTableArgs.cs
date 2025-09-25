namespace ClickHouse.Facades.Migrations;

public interface IClickHouseReplicatedTableArgs
{
	string ZooPath { get; }
	string ReplicaName { get; }
}
