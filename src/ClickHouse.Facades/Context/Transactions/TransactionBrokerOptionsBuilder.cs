using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

public class TransactionBrokerOptionsBuilder : Builder<TransactionBrokerOptions, TransactionBrokerOptionsBuilder>
{
	private OptionalValue<bool> _allowMultipleTransactions;
	private OptionalValue<bool> _autoBeginTransaction;
	private OptionalValue<bool> _autoRollbackTransaction;
	private OptionalValue<bool> _autoCommitTransaction;

	/// <summary>
	/// Allow multiple transactions within context.
	/// </summary>
	public TransactionBrokerOptionsBuilder AllowMultipleTransactions()
	{
		return WithPropertyValue(
			builder => builder._allowMultipleTransactions,
			(builder, value) => builder._allowMultipleTransactions = value,
			true);
	}

	/// <summary>
	/// Automatically begin transaction on context creation.
	/// </summary>
	public TransactionBrokerOptionsBuilder AutoBegin()
	{
		return WithPropertyValue(
			builder => builder._autoBeginTransaction,
			(builder, value) => builder._autoBeginTransaction = value,
			true);
	}

	/// <summary>
	/// Automatically rollback active transaction on context disposal.
	/// </summary>
	/// <exception cref="InvalidOperationException">Auto-commit is enabled.</exception>
	public TransactionBrokerOptionsBuilder AutoRollback()
	{
		if (_autoCommitTransaction.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to enable auto-rollback while auto-commit is enabled.");
		}

		return WithPropertyValue(
			builder => builder._autoRollbackTransaction,
			(builder, value) => builder._autoRollbackTransaction = value,
			true);
	}

	/// <summary>
	/// Automatically commit active transaction on context disposal.
	/// </summary>
	/// <exception cref="InvalidOperationException">Auto-rollback is enabled.</exception>
	public TransactionBrokerOptionsBuilder AutoCommit()
	{
		if (_autoRollbackTransaction.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to enable auto-commit while auto-rollback is enabled.");
		}

		return WithPropertyValue(
			builder => builder._autoCommitTransaction,
			(builder, value) => builder._autoCommitTransaction = value,
			true);
	}

	protected override TransactionBrokerOptions BuildCore()
	{
		return new TransactionBrokerOptions
		{
			AllowMultipleTransactions = _allowMultipleTransactions.OrElseValue(false),
			AutoBeginTransaction = _autoBeginTransaction.OrElseValue(false),
			AutoCommitTransaction = _autoCommitTransaction.OrElseValue(false),
			AutoRollbackTransaction = _autoRollbackTransaction.OrElseValue(false),
		};
	}
}
