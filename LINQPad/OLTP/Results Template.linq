<Query Kind="Program" />

void Main()
{
	
}

// You can define other methods, fields, classes and namespaces here
#region Test Methods
#endregion

#region Methods
#endregion

#region ViewModels
#endregion

#region Results
public class Result
{
	public bool IsSuccess { get; protected set; }

	public bool IsFailure => !IsSuccess;

	public IReadOnlyList<Error> Errors { get; protected set; }

	public Error? FirstError => Errors.FirstOrDefault();

	public Result()
	{
		Errors = new List<Error>();
		IsSuccess = true;
	}

	public static Result Success()
	{
		return new Result
		{
			IsSuccess = true
		};
	}

	public static Result Failure(Error error)
	{
		return new Result
		{
			IsSuccess = false,
			Errors = new List<Error> { error }
		};
	}

	public static Result Failure(IEnumerable<Error> errors)
	{
		if (errors == null || !errors.Any())
		{
			throw new ArgumentException("At least one error must be provided for a failure result.", "errors");
		}

		return new Result
		{
			IsSuccess = false,
			Errors = errors.ToList()
		};
	}

	public static Result Failure(string message)
	{
		return Failure(new Error(message));
	}

	public static Result Failure(string code, string message)
	{
		return Failure(new Error(code, message));
	}

	public static Result Combine(params Result[] results)
	{
		if (results == null || results.Length == 0)
		{
			throw new ArgumentException("At least one result must be provided to combine.", "results");
		}

		List<Error> list = results.Where((Result r) => r.IsFailure).SelectMany((Result r) => r.Errors).ToList();
		if (list.Any())
		{
			return new Result
			{
				IsSuccess = false,
				Errors = list
			};
		}

		return Success();
	}

	public Result AddError(Error error)
	{
		if (IsSuccess)
		{
			IsSuccess = false;
		}

		if (Errors is List<Error> list)
		{
			list.Add(error);
		}
		else
		{
			Errors = new List<Error>(Errors) { error };
		}

		return this;
	}

	public Result AddErrors(IEnumerable<Error> errors)
	{
		if (IsSuccess)
		{
			IsSuccess = false;
		}

		if (Errors is List<Error> list)
		{
			list.AddRange(errors);
		}
		else
		{
			Errors = new List<Error>(Errors).Concat(errors).ToList();
		}

		return this;
	}
}
public class Result<T> : Result
{
	public T? Value { get; protected set; }

	public Result()
		: this(default(T))
	{
	}

	protected Result(T? value)
	{
		Value = value;
	}

	public static Result<T> Success(T value)
	{
		return new Result<T>(value)
		{
			IsSuccess = true
		};
	}

	public new static Result<T> Failure(Error error)
	{
		return new Result<T>(default(T))
		{
			IsSuccess = false,
			Errors = new List<Error> { error }
		};
	}

	public new static Result<T> Failure(IEnumerable<Error> errors)
	{
		if (errors == null || !errors.Any())
		{
			throw new ArgumentException("At least one error must be provided for a failure result.", "errors");
		}

		return new Result<T>(default(T))
		{
			IsSuccess = false,
			Errors = errors.ToList()
		};
	}

	public new static Result<T> Failure(string message)
	{
		return Failure(new Error(message));
	}

	public new static Result<T> Failure(string code, string message)
	{
		return Failure(new Error(code, message));
	}

	public new static Result<T> Combine(params Result[] results)
	{
		if (results == null || results.Length == 0)
		{
			throw new ArgumentException("At least one result must be provided to combine.", "results");
		}

		List<Error> list = results.Where((Result r) => r.IsFailure).SelectMany((Result r) => r.Errors).ToList();
		if (list.Any())
		{
			return new Result<T>(default(T))
			{
				IsSuccess = false,
				Errors = list
			};
		}

		return Success(default(T));
	}

	public static implicit operator Result<T>(T value)
	{
		return Success(value);
	}

	public Result<TNext> Map<TNext>(Func<T, TNext> func)
	{
		if (base.IsFailure)
		{
			return Result<TNext>.Failure((IEnumerable<Error>)base.Errors);
		}

		return Result<TNext>.Success(func(Value));
	}

	public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> func)
	{
		if (base.IsFailure)
		{
			return Result<TNext>.Failure((IEnumerable<Error>)base.Errors);
		}

		return func(Value);
	}

	public new Result<T> AddError(Error error)
	{
		base.AddError(error);
		return this;
	}

	public new Result<T> AddErrors(IEnumerable<Error> errors)
	{
		base.AddErrors(errors);
		return this;
	}

	public Result<T> WithValue(T value)
	{
		if (base.IsSuccess)
		{
			Value = value;
			return this;
		}

		throw new InvalidOperationException("Cannot set the value of a failed result.  The result must be successful.");
	}
}
public class Error : IEquatable<Error>
{
	public string Code { get; }

	public string Message { get; }

	public Error(string message)
		: this(string.Empty, message)
	{
	}

	public Error(string code, string message)
	{
		Code = code ?? string.Empty;
		Message = message ?? string.Empty;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Error);
	}

	public bool Equals(Error? other)
	{
		if (other == null)
		{
			return false;
		}

		if (Code == other.Code)
		{
			return Message == other.Message;
		}

		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 23 + Code.GetHashCode()) * 23 + Message.GetHashCode();
	}

	public static bool operator ==(Error? left, Error? right)
	{
		return EqualityComparer<Error>.Default.Equals(left, right);
	}

	public static bool operator !=(Error? left, Error? right)
	{
		return !(left == right);
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(Code))
		{
			return Message;
		}

		return Code + ": " + Message;
	}
}
#endregion