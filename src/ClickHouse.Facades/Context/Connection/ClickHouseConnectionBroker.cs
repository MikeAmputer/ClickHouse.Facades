﻿using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Adapters;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

internal class ClickHouseConnectionBroker : IClickHouseConnectionBroker
{
	private const string UseSessionConnectionStringParameter = "usesession";

	private readonly ClickHouseConnection _connection;
	private readonly bool _sessionEnabled;
	private readonly ICommandExecutionStrategy _commandExecutionStrategy;
	private readonly IClickHouseCommandExecutionListener? _commandExecutionListener;

	public ClickHouseConnectionBroker(ConnectionBrokerParameters brokerParameters)
	{
		if (_connection != null)
		{
			throw new InvalidOperationException($"{GetType()} is already connected.");
		}

		if (brokerParameters == null)
		{
			throw new ArgumentNullException(nameof(brokerParameters));
		}

		_connection = brokerParameters.Connection
			?? throw new ArgumentNullException(nameof(brokerParameters.Connection));

		_commandExecutionStrategy = brokerParameters.CommandExecutionStrategy
			?? throw new ArgumentNullException(nameof(brokerParameters.CommandExecutionStrategy));

		_commandExecutionListener = brokerParameters.CommandExecutionListener;

		_sessionEnabled = brokerParameters.Connection.ConnectionString
			.GetConnectionStringParameters()
			.Contains(new KeyValuePair<string, string?>(UseSessionConnectionStringParameter, true.ToString()));
	}

	public string? ServerVersion => _connection.ServerVersion;

	public string? ServerTimezone => _connection.ServerTimezone;

	public ClickHouseCommand CreateCommand()
	{
		ThrowIfNotConnected();

		return _connection.CreateCommand();
	}

	public async Task<object> ExecuteScalarAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = query;
		SetParameters(command, parameters);

		var result = await _commandExecutionStrategy
			.ExecuteScalarAsync(_connection, command, cancellationToken);

		await PublishExecutedCommand(command, cancellationToken);

		return result;
	}

	public async Task<int> ExecuteNonQueryAsync(
		string statement,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = statement;
		SetParameters(command, parameters);

		var result = await _commandExecutionStrategy
			.ExecuteNonQueryAsync(_connection, command, cancellationToken);

		await PublishExecutedCommand(command, cancellationToken);

		return result;
	}

	public async Task<DbDataReader> ExecuteReaderAsync(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await using var command = CreateCommand();
		command.CommandText = query;
		SetParameters(command, parameters);

		var result = await _commandExecutionStrategy
			.ExecuteDataReaderAsync(_connection, command, cancellationToken);

		await PublishExecutedCommand(command, cancellationToken);

		return result;
	}

	public DataTable ExecuteDataTable(
		string query,
		Dictionary<string, object>? parameters,
		CancellationToken cancellationToken)
	{
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

	public async Task<long> BulkInsertAsync(
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

	public Task SetSessionParameterAsync(string parameterName, object value)
	{
		if (!_sessionEnabled)
		{
			throw new InvalidOperationException(
				"Unable to set session parameter while sessions are not enabled in the current context.");
		}

		return _connection.ExecuteStatementAsync($"set {parameterName} = '{value}'");
	}

	public async Task BeginTransactionAsync()
	{
		if (!_sessionEnabled)
		{
			throw new InvalidOperationException(
				"Transactions unavailable while sessions are not enabled in the current context.");
		}

		await using var command = CreateCommand();
		command.CommandText = "BEGIN TRANSACTION";

		await command.ExecuteNonQueryAsync();
	}

	public async Task CommitTransactionAsync()
	{
		if (!_sessionEnabled)
		{
			throw new InvalidOperationException(
				"Transactions unavailable while sessions are not enabled in the current context.");
		}

		await using var command = CreateCommand();
		command.CommandText = "COMMIT";

		await command.ExecuteNonQueryAsync();
	}

	public async Task RollbackTransactionAsync()
	{
		if (!_sessionEnabled)
		{
			throw new InvalidOperationException(
				"Transactions unavailable while sessions are not enabled in the current context.");
		}

		await using var command = CreateCommand();
		command.CommandText = "ROLLBACK";

		await command.ExecuteNonQueryAsync();
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

	private async Task PublishExecutedCommand(ClickHouseCommand command, CancellationToken cancellationToken)
	{
		if (_commandExecutionListener != null)
		{
			await _commandExecutionListener.ProcessExecutedCommand(command, cancellationToken);
		}
	}
}
