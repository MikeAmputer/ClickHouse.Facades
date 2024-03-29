﻿using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Adapters;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

internal class ClickHouseConnectionBroker
{
	private const string UseSessionConnectionStringParameter = "usesession";

	private readonly ClickHouseConnection _connection;
	private readonly bool _sessionEnabled;

	public ClickHouseConnectionBroker(ClickHouseConnection connection)
	{
		if (_connection != null)
		{
			throw new InvalidOperationException($"{GetType()} is already connected.");
		}

		_connection = connection ?? throw new ArgumentNullException(nameof(connection));

		_sessionEnabled = connection.ConnectionString
			.GetConnectionStringParameters()
			.Contains(new KeyValuePair<string, string?>(UseSessionConnectionStringParameter, true.ToString()));
	}

	internal virtual string? ServerVersion => _connection.ServerVersion;

	internal virtual string? ServerTimezone => _connection.ServerTimezone;

	internal virtual ClickHouseCommand CreateCommand()
	{
		ThrowIfNotConnected();

		return _connection.CreateCommand();
	}

	internal virtual async Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = query;
		SetParameters(command, parameters);

		return await command.ExecuteScalarAsync(cancellationToken);
	}

	internal virtual async Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = statement;
		SetParameters(command, parameters);

		return await command.ExecuteNonQueryAsync(cancellationToken);
	}

	internal virtual async Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = query;
		SetParameters(command, parameters);

		return await command.ExecuteReaderAsync(cancellationToken);
	}

	internal virtual DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		cancellationToken.ThrowIfCancellationRequested();

		using var command = CreateCommand();
		using var adapter = new ClickHouseDataAdapter();

		command.CommandText = query;
		SetParameters(command, parameters);

		adapter.SelectCommand = command;

		var dataTable = new DataTable();
		adapter.Fill(dataTable);
		return dataTable;
	}

	internal virtual async Task<long> BulkInsertAsync(
		string destinationTable,
		Func<ClickHouseBulkCopy, Task> saveAction,
		int batchSize,
		int maxDegreeOfParallelism,
		IReadOnlyCollection<string>? columnNames = null)
	{
		ThrowIfNotConnected();

		if (destinationTable.IsNullOrWhiteSpace())
		{
			throw new ArgumentException($"{nameof(destinationTable)} is null or whitespace.");
		}

		if (batchSize < 1)
		{
			throw new ArgumentException($"{nameof(batchSize)} is lower than 1.");
		}

		switch (maxDegreeOfParallelism)
		{
			case < 1:
				throw new ArgumentException($"{nameof(maxDegreeOfParallelism)} is lower than 1.");
			case > 1 when _sessionEnabled:
				throw new InvalidOperationException($"Sessions are not compatible with parallel insertion.");
		}

		using var bulkCopyInterface = new ClickHouseBulkCopy(_connection)
		{
			DestinationTableName = destinationTable,
			BatchSize = batchSize,
			MaxDegreeOfParallelism = maxDegreeOfParallelism,
			ColumnNames = columnNames,
		};

		await bulkCopyInterface.InitAsync();
		await saveAction(bulkCopyInterface);

		return bulkCopyInterface.RowsWritten;
	}

	private void SetParameters(ClickHouseCommand command, Dictionary<string, object>? parameters)
	{
		if (parameters == null)
		{
			return;
		}

		foreach (var (key, value) in parameters)
		{
			command.AddParameter(key, value);
		}
	}

	private void ThrowIfNotConnected()
	{
		if (_connection == null)
		{
			throw new InvalidOperationException($"{GetType()} is not connected.");
		}
	}
}
