using ClickHouse.Driver.ADO;
using ClickHouse.Facades.Utility;

namespace ClickHouse.Facades;

internal sealed class ClickHouseClientSettingsBuilder
	: Builder<ClickHouseClientSettings, ClickHouseClientSettingsBuilder>
{
	private OptionalValue<ClickHouseClientSettings> _baseSettings;
	private OptionalValue<string> _connectionString;

	private OptionalValue<HttpClient> _httpClient;
	private OptionalValue<IHttpClientFactory> _httpClientFactory;
	private OptionalValue<string> _httpClientName;

	private OptionalValue<bool> _useSession;
	private OptionalValue<bool> _useFormDataParameters;

	private OptionalValue<IDictionary<string, object>> _customSettings;

	public ClickHouseClientSettingsBuilder WithHttpClientFactory(
		IHttpClientFactory httpClientFactory,
		string httpClientName)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentException.ThrowIfNullOrWhiteSpace(httpClientName);

		if (_httpClient.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to set IHttpClientFactory and HttpClient simultaneously.");
		}

		WithPropertyValue(
			builder => builder._httpClientName,
			(builder, value) => builder._httpClientName = value,
			httpClientName);

		return WithPropertyValue(
			builder => builder._httpClientFactory,
			(builder, value) => builder._httpClientFactory = value,
			httpClientFactory);
	}

	public ClickHouseClientSettingsBuilder WithHttpClient(HttpClient httpClient)
	{
		ArgumentNullException.ThrowIfNull(httpClient);

		if (_httpClientFactory.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to set IHttpClientFactory and HttpClient simultaneously.");
		}

		return WithPropertyValue(
			builder => builder._httpClient,
			(builder, value) => builder._httpClient = value,
			httpClient);
	}

	public ClickHouseClientSettingsBuilder UseFormDataParameters()
	{
		return WithPropertyValue(
			builder => builder._useFormDataParameters,
			(builder, value) => builder._useFormDataParameters = value,
			true);
	}

	public ClickHouseClientSettingsBuilder UseSession()
	{
		return WithPropertyValue(
			builder => builder._useSession,
			(builder, value) => builder._useSession = value,
			true);
	}

	public ClickHouseClientSettingsBuilder WithCustomSettings(IDictionary<string, object> customSettings)
	{
		return WithPropertyValue(
			builder => builder._customSettings,
			(builder, value) => builder._customSettings = value,
			customSettings);
	}

	public ClickHouseClientSettingsBuilder WithClientSettings(ClickHouseClientSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		if (_connectionString.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to setup client settings from both connection string and base settings at the same time.");
		}

		return WithPropertyValue(
			builder => builder._baseSettings,
			(builder, value) => builder._baseSettings = value,
			settings);
	}

	public ClickHouseClientSettingsBuilder WithConnectionString(string connectionString)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

		if (_baseSettings.HasValue)
		{
			throw new InvalidOperationException(
				"Unable to setup client settings from both connection string and base settings at the same time.");
		}

		return WithPropertyValue(
			builder => builder._connectionString,
			(builder, value) => builder._connectionString = value,
			connectionString);
	}

	protected override ClickHouseClientSettings BuildCore()
	{
		var baseSettings = _baseSettings.OrElse(() =>
			new ClickHouseClientSettings(_connectionString.OrElse(() =>
				throw new InvalidOperationException(
					"Client settings missing constructor argument (connection string or base settings)."))))!;

		return new ClickHouseClientSettings(baseSettings)
		{
			HttpClient = _httpClient.OrElseValue(baseSettings.HttpClient),
			HttpClientFactory = _httpClientFactory.OrElseValue(baseSettings.HttpClientFactory),
			HttpClientName = _httpClientName.OrElseValue(baseSettings.HttpClientName),

			UseSession = _useSession.OrElseValue(baseSettings.UseSession),
			UseFormDataParameters = _useFormDataParameters.OrElseValue(baseSettings.UseFormDataParameters),

			CustomSettings = _customSettings.OrElseValue(baseSettings.CustomSettings),
		};
	}
}
