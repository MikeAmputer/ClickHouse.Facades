namespace ClickHouse.Facades.Utility;

internal readonly struct OptionalValue<TValue>
{
	internal bool HasValue { get; }

	private TValue? Value { get; }

	private OptionalValue(TValue? value) : this()
	{
		Value = value;
		HasValue = true;
	}

	public static implicit operator OptionalValue<TValue>(TValue? value) => new(value);

	public static explicit operator TValue?(OptionalValue<TValue> optionalValue) => optionalValue.Value;

	internal TValue? OrElseValue(TValue? elseValue)
	{
		return OrElse(() => elseValue);
	}

	internal TValue? OrDefault()
	{
		return OrElse(() => default);
	}

	internal TValue? OrThrow()
	{
		return OrElse(() =>
			throw new InvalidOperationException("Value not set."));
	}

	internal TValue NotNullOrThrow()
	{
		return ThrowIfNull();
	}

	internal TValue? OrElse(Func<TValue?> elseValueProvider)
	{
		ExceptionHelpers.ThrowIfNull(elseValueProvider);

		return HasValue ? Value : elseValueProvider();
	}

	internal string ToString(Func<TValue?, string> stringProvider, string fallbackValue = "")
	{
		ExceptionHelpers.ThrowIfNull(stringProvider);

		return HasValue ? stringProvider(Value) : fallbackValue;
	}

	internal string NotNullToString(Func<TValue, string> stringProvider, string fallbackValue)
	{
		ExceptionHelpers.ThrowIfNull(stringProvider);

		return HasValue ? stringProvider(ThrowIfNull()) : fallbackValue;
	}

	internal string NotNullToString(Func<TValue, string> stringProvider)
	{
		ExceptionHelpers.ThrowIfNull(stringProvider);

		return stringProvider(ThrowIfNull());
	}

	private TValue ThrowIfNull()
	{
		if (!HasValue)
		{
			throw new InvalidOperationException("Value not set.");
		}

		if (Value == null)
		{
			throw new InvalidOperationException("Value is null.");
		}

		return Value;
	}
}
