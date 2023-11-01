using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClickHouse.Facades.Testing;

public class ClickHouseFacadesTestsCore
{
	private readonly IServiceCollection _serviceCollection;
	private IServiceProvider _serviceProvider;


	[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
	protected ClickHouseFacadesTestsCore()
	{
		_serviceCollection = new ServiceCollection();
		SetupServiceCollection(_serviceCollection);
		_serviceProvider = _serviceCollection.BuildServiceProvider();
	}

	protected T GetService<T>() where T : notnull
	{
		return _serviceProvider.GetRequiredService<T>();
	}

	/// <summary>
	/// Is called in constructor. Should never access class members.
	/// </summary>
	protected virtual void SetupServiceCollection(IServiceCollection services)
	{

	}

	protected void UpdateServiceCollection(Action<IServiceCollection> action)
	{
		action(_serviceCollection);

		_serviceProvider = _serviceCollection.BuildServiceProvider();
	}

	protected void MockExecuteNonQuery<TContext>(Predicate<string> sqlPredicate, Func<int> resultProvider)
		where TContext : ClickHouseContext<TContext>
	{
		GetService<ClickHouseConnectionResponseProducer<TContext>>()
			.Add(TestQueryType.ExecuteNonQuery, sqlPredicate, () => resultProvider());
	}

	protected void MockExecuteScalar<TContext>(Predicate<string> sqlPredicate, Func<object> resultProvider)
		where TContext : ClickHouseContext<TContext>
	{
		GetService<ClickHouseConnectionResponseProducer<TContext>>()
			.Add(TestQueryType.ExecuteScalar, sqlPredicate, resultProvider);
	}

	protected void MockExecuteReader<TContext, TResult>(
		Predicate<string> sqlPredicate,
		IEnumerable<TResult> rows,
		params (string ColumnName, Type DataType, Func<TResult, object> PropertySelector)[] columns)
		where TContext : ClickHouseContext<TContext>
	{
		var dataTable = new DataTable();

		foreach (var column in columns)
		{
			dataTable.Columns.Add(column.ColumnName, column.DataType);
		}

		foreach (var row in rows)
		{
			var dataRow = dataTable.NewRow();

			foreach (var column in columns)
			{
				dataRow[column.ColumnName] = column.PropertySelector(row) ?? DBNull.Value;
			}

			dataTable.Rows.Add(dataRow);
		}

		var dataReader = dataTable.CreateDataReader();

		GetService<ClickHouseConnectionResponseProducer<TContext>>()
			.Add(TestQueryType.ExecuteReader, sqlPredicate, () => dataReader);
	}

	protected IReadOnlyCollection<ClickHouseTestResponse> GetClickHouseResponses<TContext>()
		where TContext : ClickHouseContext<TContext>
	{
		return GetService<ClickHouseConnectionTracker<TContext>>().GetAllRecords();
	}

	protected ClickHouseTestResponse GetClickHouseResponse<TContext>(int index)
		where TContext : ClickHouseContext<TContext>
	{
		return GetService<ClickHouseConnectionTracker<TContext>>().GetRecord(index);
	}

	protected int GetClickHouseResponsesCount<TContext>()
		where TContext : ClickHouseContext<TContext>
	{
		return GetService<ClickHouseConnectionTracker<TContext>>().RecordsCount;
	}

	protected void MockServerVersion<TContext>(Func<string?> valueProvider)
		where TContext : ClickHouseContext<TContext>
	{
		GetService<ClickHouseConnectionResponseProducer<TContext>>().SetServerVersionProvider(valueProvider);
	}

	protected void MockServerTimezone<TContext>(Func<string?> valueProvider)
		where TContext : ClickHouseContext<TContext>
	{
		GetService<ClickHouseConnectionResponseProducer<TContext>>().SetServerTimezoneProvider(valueProvider);
	}

	protected void MockFacadeAbstraction<TAbstraction>(TAbstraction mock)
		where TAbstraction : class
	{
		UpdateServiceCollection(services =>
		{
			services.RemoveAll<TAbstraction>();
			services.AddTransient(_ => mock);
		});
	}
}
