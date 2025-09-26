using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades.Migrations;

public sealed class ClickHouseReplicatedTableArgs
{
	internal string ZooPath { get; }
	internal string ReplicaName { get; }

	public ClickHouseReplicatedTableArgs(string zooPath, string replicaName)
	{
		if (zooPath.IsNullOrWhiteSpace())
		{
			throw new ArgumentException("Parameter is null or empty.", nameof(zooPath));
		}

		if (replicaName.IsNullOrWhiteSpace())
		{
			throw new ArgumentException("Parameter is null or empty.", nameof(replicaName));
		}

		ZooPath = zooPath;
		ReplicaName = replicaName;
	}
}
