namespace ClickHouse.Facades.Utility;

public abstract class Builder<TResult, TBuilder>
	where TBuilder : Builder<TResult, TBuilder>, new()
{
	internal Builder() { }

	public static TBuilder Create => new();

	private bool _isBuilt = false;

	internal TBuilder WithPropertyValue<TValue>(
		Func<TBuilder, OptionalValue<TValue>> getter,
		Action<TBuilder, OptionalValue<TValue>> setter,
		TValue value,
		bool overrideAllowed = false)
	{
		ExceptionHelpers.ThrowIfNull(getter);
		ExceptionHelpers.ThrowIfNull(setter);

		ThrowIfBuilt();

		if (!overrideAllowed && getter((TBuilder) this).HasValue)
		{
			throw new InvalidOperationException("Builder property has been set already.");
		}

		var result = (TBuilder) this;
		setter(result, value);
		return result;
	}

	internal TBuilder WithNamedArgument<TValue>(
		Func<TBuilder, Dictionary<string, TValue>> getter,
		string key,
		TValue value)
	{
		ExceptionHelpers.ThrowIfNull(getter);

		ThrowIfBuilt();

		var dict = getter((TBuilder) this);

		if (!dict.TryAdd(key, value))
		{
			throw new InvalidOperationException($"Argument with name {key} is already in dictionary.");
		}

		return (TBuilder) this;
	}

	protected abstract TResult BuildCore();

	internal TResult Build()
	{
		_isBuilt = true;
		return BuildCore();
	}

	protected void ThrowIfBuilt()
	{
		if (_isBuilt)
		{
			throw new InvalidOperationException($"Entity {GetType()} has been built already.");
		}
	}
}
