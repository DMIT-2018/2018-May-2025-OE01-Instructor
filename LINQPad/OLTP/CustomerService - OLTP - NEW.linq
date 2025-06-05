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
  <NuGetReference>BYSResults</NuGetReference>
</Query>

// 	Lightweight result types for explicit success/failure 
//	 handling in .NET applications.
using BYSResults;

// —————— PART 1: Main → UI ——————
//	Driver is responsible for orchestrating the flow by calling 
//	various methods and classes that contain the actual business logic 
//	or data processing operations.
void Main()
{
	CodeBehind codeBehind = new CodeBehind(this); // “this” is LINQPad’s auto Context
	
	//Fail Tests
	// Rule: either the last name or phone number must be provided.
	codeBehind.GetCustomers(string.Empty, string.Empty);
	codeBehind.ErrorDetails.Dump("either the last name or phone number must be provided.");
	
	// rule: either the last name or the phone number must be valid (no search results).
	codeBehind.GetCustomers("zzzz","999999");
	codeBehind.ErrorDetails.Dump("either the last name or the phone number must be valid (no search results).");
	
	// Pass: both the last name and phone number were provided and valid
	codeBehind.GetCustomers("Sm","558");
	codeBehind.Customers.Dump("Pass - Valid last name & phone number.");

	//Pass: last name provided
	codeBehind.GetCustomers("Sm", string.Empty);
	codeBehind.Customers.Dump("Pass - Valid last name.");

	//Pass: Phone number provided
	codeBehind.GetCustomers(string.Empty, "558");
	codeBehind.Customers.Dump("Pass - Valid phone number.");

}

// ———— PART 2: Code Behind → Code Behind Method ————
// This region contains methods used to test the functionality
// of the application's business logic and ensure correctness.
// NOTE: This class functions as the code-behind for your Blazor pages
#region Code Behind Methods
public class CodeBehind(TypedDataContext context)
{
	#region Supporting Members (Do not modify)
	// exposes the collected error details
	public List<string> ErrorDetails => errorDetails;

	// Mock injection of the service into our code-behind.
	// You will need to refactor this for proper dependency injection.
	// NOTE: The TypedDataContext must be passed in.
	private readonly Library CustomerService = new Library(context);
	#endregion

	#region Fields from Blazor Page Code-Behind
	// feedback message to display to the user.
	private string feedbackMessage = string.Empty;
	// collected error details.
	private List<string> errorDetails = new();
	// general error message.
	private string errorMessage = string.Empty;
	#endregion  
	
	//List to hold the CustomerSearchView Results
		//Default just sets the value to the data type default
		// ! says to the code, ignore any nulls I don't care.
		// Do not use ! unless you know it doesn't matter
			//This means at this point in your education, 
			//don't use it unless you ask or have been told it's okay.
	public List<CustomerSearchView> Customers = default!;
	
	//Same naming and same parameters as the method in your library.
	public void GetCustomers(string lastName, string phone)
	{
		//clear previous error details and messages
		//Always start with this, or you end up with
			//repeat error messages or feedback
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = string.Empty;
		
		//Wrap the call to the service in a try/catch to handle any unexcepted exceptions
		try 
		{
			var result = CustomerService.GetCustomers(lastName, phone);
			if(result.IsSuccess)
				Customers = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			//capture any unexpected exceptions to our error message
			errorMessage = ex.Message;
		}
	}

}
#endregion

// ———— PART 3: Database Interaction Method → Service Library Method ————
//	This region contains support methods for testing
#region Methods
public class Library
{
	#region Data Context Setup
	// The LINQPad auto-generated TypedDataContext instance used to query and manipulate data.
	private readonly TypedDataContext _context;

	// The TypedDataContext provided by LINQPad for database access.
	// Store the injected context for use in library methods
	// NOTE:  This constructor is simular to the constuctor in your service
	public Library(TypedDataContext context)
	{
		_context = context
					?? throw new ArgumentNullException(nameof(context));
	}
	#endregion

	public Result<List<CustomerSearchView>> GetCustomers(string lastName, string phone)
	{
		//Result should be given the same type as the expected return type
		var result = new Result<List<CustomerSearchView>>();
		//rule: Both last name and phone number cannot be empty
		if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(phone))
		{
			result.AddError(new Error("Missing Information", "Please provide either a last name and/or a phone number"));
			return result;
		}


		//rule: RemoveFromViewFlag cannot be true
		var customers = _context.Customers
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

		if (customers.Count <= 0)
		{
			result.AddError(new Error("No Customers", "No customer were found."));
			return result;
		}
		return result.WithValue(customers);
	}
}
#endregion

// ———— PART 4: View Models → Service Library View Model ————
//	This region includes the view models used to 
//	represent and structure data for the UI.
#region View Models
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

//	This region includes support methods
#region Support Method
// Converts a list of error objects into their string representations.
public static List<string> GetErrorMessages(List<Error> errorMessage)
{
	// Initialize a new list to hold the extracted error messages
	List<string> errorList = new();

	// Iterate over each Error object in the incoming list
	foreach (var error in errorMessage)
	{
		// Convert the current Error to its string form and add it to errorList
		errorList.Add(error.ToString());
	}

	// Return the populated list of error message strings
	return errorList;
}
#endregion