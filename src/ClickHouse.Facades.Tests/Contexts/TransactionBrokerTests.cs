using Moq;

namespace ClickHouse.Facades.Tests;

[TestClass]
public class TransactionBrokerTests
{
	[TestMethod]
	public async Task DefaultOptions_Begin_BeginTransaction_NoOtherCalls()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginCommit_BeginCommitTransaction_NoOtherCalls()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.CommitAsync();
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.CommitTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginRollback_BeginRollbackTransaction_NoOtherCalls()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.RollbackAsync();
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.RollbackTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginBegin_Throws()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transactionBroker.BeginAsync());
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginCommitRollback_Throws()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.CommitAsync();

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transactionBroker.RollbackAsync());
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.CommitTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginRollbackCommit_Throws()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.RollbackAsync();

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transactionBroker.CommitAsync());
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.RollbackTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginCommitBegin_Throws()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.CommitAsync();

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transactionBroker.BeginAsync());
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.CommitTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task DefaultOptions_BeginRollbackBegin_Throws()
	{
		var options = new TransactionBrokerOptions();
		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.RollbackAsync();

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transactionBroker.BeginAsync());
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.RollbackTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AllowMultiple_BeginCommitBegin_Success()
	{
		var options = new TransactionBrokerOptions
		{
			AllowMultipleTransactions = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.CommitAsync();
			await transactionBroker.BeginAsync();
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Exactly(2));
		connectionBroker.Verify(b => b.CommitTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AllowMultiple_BeginRollbackBegin_Success()
	{
		var options = new TransactionBrokerOptions
		{
			AllowMultipleTransactions = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{
			await transactionBroker.BeginAsync();
			await transactionBroker.RollbackAsync();
			await transactionBroker.BeginAsync();
		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Exactly(2));
		connectionBroker.Verify(b => b.RollbackTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AutoBegin_CallNothing_BeginTransaction()
	{
		var options = new TransactionBrokerOptions
		{
			AutoBeginTransaction = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		// ReSharper disable once UnusedVariable
		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{

		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AutoBeginCommit_CallNothing_BeginCommitTransaction()
	{
		var options = new TransactionBrokerOptions
		{
			AutoBeginTransaction = true,
			AutoCommitTransaction = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		// ReSharper disable once UnusedVariable
		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{

		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.CommitTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AutoBeginRollback_CallNothing_BeginRollbackTransaction()
	{
		var options = new TransactionBrokerOptions
		{
			AutoBeginTransaction = true,
			AutoRollbackTransaction = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		// ReSharper disable once UnusedVariable
		await using (var transactionBroker = await TransactionBroker.Create(connectionBroker.Object, options))
		{

		}

		connectionBroker.Verify(b => b.BeginTransactionAsync(), Times.Once);
		connectionBroker.Verify(b => b.RollbackTransactionAsync(), Times.Once);
		connectionBroker.VerifyNoOtherCalls();
	}

	[TestMethod]
	public async Task AutoCommitRollback_ThrowsOnCreation()
	{
		var options = new TransactionBrokerOptions
		{
			AutoCommitTransaction = true,
			AutoRollbackTransaction = true,
		};

		var connectionBroker = CreateConnectionBrokerMock();

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
			TransactionBroker.Create(connectionBroker.Object, options));

		connectionBroker.VerifyNoOtherCalls();
	}

	private Mock<IClickHouseConnectionBroker> CreateConnectionBrokerMock()
	{
		Mock<IClickHouseConnectionBroker> mock = new();

		mock.Setup(m => m.BeginTransactionAsync()).Verifiable();
		mock.Setup(m => m.CommitTransactionAsync()).Verifiable();
		mock.Setup(m => m.RollbackTransactionAsync()).Verifiable();

		return mock;
	}
}
