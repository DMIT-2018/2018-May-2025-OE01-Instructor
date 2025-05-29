<Query Kind="Program">
  <Connection>
    <ID>670ecc76-0701-4a3c-a369-189a5cc72796</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Server>MOMSDESKTOP\SQLEXPRESS</Server>
    <Database>OLTP-DMIT2018</Database>
    <DisplayName>OLTP-DMIT2018-Entity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	Console.WriteLine("=========================");
	Console.WriteLine("---- Get Customer By ID - Success Tests");
	Console.WriteLine("=========================");
	var results = GetCustomer_ByID(360);
	//If we expect it to pass and it is Success, then the test passed.
	if(results.IsSuccess)
		results.Value.Dump("Pass - Get Customer by valid ID - Customer Found");
	else
		results.Errors.Dump("Fail - Errors should not be thrown");
	Console.WriteLine("=========================");
	Console.WriteLine("---- Get Customer By ID - Failure Tests");
	Console.WriteLine("=========================");
	var results1 = GetCustomer_ByID(0);
	//If we expect it to fail and IsFailure = true, then the test passed.
	if(results1.IsFailure)
		results1.Errors.Dump("Pass - Get Customer by Invalid ID - 0 ID given");
	else
		"Fail - Expected error not produced.".Dump();
	Console.WriteLine("=========================");
	Console.WriteLine("---- Get Customers - Success Tests");
	Console.WriteLine("=========================");
	var results2 = GetCustomers("","444");
	if(results2.IsSuccess)
		results2.Value.Dump("Pass - Get Customers by Phone Number - Customers Found");
	else
		results2.Errors.Dump("Fail - Error should not be thrown");

	var results3 = GetCustomers("Smith", "");
	if (results3.IsSuccess)
		results3.Value.Dump("Pass - Get Customers by Last Name - Customers Found");
	else
		results3.Errors.Dump("Fail - Error should not be thrown");
	var results4 = GetCustomers("Smith", "444");
	if (results3.IsSuccess)
		results4.Value.Dump("Pass - Get Customers by Last Name and phone number - Customers Found");
	else
		results4.Errors.Dump("Fail - Error should not be thrown");
	//GetCustomers("","").Dump();
	
	//GetLookupValues("Province").Dump();
	
	
	
}

// You can define other methods, fields, classes and namespaces here
#region Test Methods
#endregion

#region Methods
public Result<List<CustomerSearchView>> GetCustomers(string lastName, string phone)
{
	//Result should be given the same type as the expected return type
	var result = new Result<List<CustomerSearchView>>();
	//rule: Both last name and phone number cannot be empty
	if(string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(phone))
	{
		result.AddError(new Error("Missing Information","Please provide either a last name and/or a phone number"));
		return result;
	}
		
	
	//rule: RemoveFromViewFlag cannot be true
	var customers = Customers
		.Where(x => !x.RemoveFromViewFlag 
						&&
				(
					(
						!string.IsNullOrWhiteSpace(lastName)
							&& 
						x.LastName.ToLower().Contains(lastName.ToLower())
					) 
					|| 
					(
						!string.IsNullOrWhiteSpace(phone) 
							&& 
						x.Phone.Contains(phone)
					)
				)
			)
		.OrderBy(x => x.ProvState.Name)
		.Select(x => new CustomerSearchView
		{
			CustomerID = x.CustomerID,
			FirstName = x.FirstName,
			LastName = x.LastName,
			CityProv = $"{x.City}, {x.ProvState.Name}",
			Phone = x.Phone,
			Email = x.Email,
			StatusID = x.StatusID,
			Status = x.Status.Name,
			TotalSales = x.Invoices.Sum(i => i.SubTotal + i.Tax)
		})
		.ToList();
		
	if(customers.Count <= 0)
	{
		result.AddError(new Error("No Customers", "No customer were found."));
		return result;
	}
	return result.WithValue(customers);
}

public Result<CustomerEditView> GetCustomer_ByID(int customerID)
{
	var result = new Result<CustomerEditView>();
	//rule: CustomerID must be valid
	if(customerID <= 0)
	{
		result.AddError(new Error("Missing Information","Please provide a valid Customer ID."));
		return result;
	}
		
		
	var customer = Customers
			.Where(x => !x.RemoveFromViewFlag
						&& x.CustomerID == customerID)
			.Select(x => new CustomerEditView
			{
				CustomerID = x.CustomerID,
				FirstName = x.FirstName,
				//Added this field and HasInvoices to show that not all values
				//in a ViewModel need to be for display
				//Sometimes we just need to keep or bring in values 
				//to use for logic
				OrginalFirstName = x.FirstName,
				LastName = x.LastName,
				Address1 = x.Address1,
				Address2 = x.Address2,
				City = x.City,
				ProvStateID = x.ProvStateID,
				CountryID = x.CountryID,
				PostalCode = x.PostalCode,
				Phone = x.Phone,
				Email = x.Email,
				StatusID = x.StatusID,
				HasInvoices = x.Invoices.Any(),
				RemoveFromViewFlag = x.RemoveFromViewFlag

			})
			.FirstOrDefault();
			
	if(customer == null)
	{
		result.AddError(new Error("No Customer", $"No customer was found with ID: {customerID}"));
		return result;
	}
	
	//return the result
	return result.WithValue(customer);
}

public List<LookupView> GetLookupValues(string categoryName)
{
	//rule: categoryName must not be null or whitespace
	if(string.IsNullOrWhiteSpace(categoryName))
		throw new ArgumentNullException("Please provide a category name.");
	//rule: the Lookup category must exist
	if(!Categories.Any(x => x.CategoryName.ToLower() == categoryName.ToLower()))
		throw new ArgumentException($"{categoryName} is not a valid lookup category.");
	return Lookups
			.Where(x => x.Category.CategoryName.ToLower() == categoryName.ToLower()
					&& !x.RemoveFromViewFlag)
			.Select(x => new LookupView
			{
				LookupID = x.LookupID,
				Name = x.Name,
				RemoveFromViewFlag = x.RemoveFromViewFlag
			})
			.OrderBy(x => x.Name)
			.ToList();
}
#endregion

#region Support Methods
public Exception GetInnerException(System.Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

#region ViewModels
public class CustomerSearchView
{
	public int CustomerID { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string CityProv { get; set; }
	public string Phone { get; set; }
	public string Email { get; set; }
	public int StatusID { get; set; }
	public string Status { get; set; }
	public decimal TotalSales { get; set; }
}
public class CustomerEditView
{
	public int CustomerID { get; set; }
	public string FirstName { get; set; }
	public string OrginalFirstName { get; set; }
	public string LastName { get; set; }
	public string Address1 { get; set; }
	public string Address2 { get; set; }
	public string City { get; set; }
	public int ProvStateID { get; set; }
	public int CountryID { get; set; }
	public string PostalCode { get; set; }
	public string Phone { get; set; }
	public string Email { get; set; }
	public int StatusID { get; set; }
	public bool HasInvoices { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
public class LookupView
{
	public int LookupID { get; set; }
	public string Name { get; set; }
	public bool RemoveFromViewFlag { get; set; }
}
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