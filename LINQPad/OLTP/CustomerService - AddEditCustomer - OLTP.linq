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
	
	//GetCustomer_ByID Tests
	
	//Fail
	//Rule: Customer ID must be greater than 0
	codeBehind.GetCustomer_ByID(0);
	codeBehind.ErrorDetails.Dump("Customer ID must be greater than zero");
	
	//Rule: Customer ID must be valid
	codeBehind.GetCustomer_ByID(100000);
	codeBehind.ErrorDetails.Dump("Customer not found for ID 100000");
	
	//Pass: valid Customer ID
	codeBehind.GetCustomer_ByID(360);
	codeBehind.Customer.Dump("Pass - Valid Customer ID");
	
	//GetLookupValues Tests
	//Fail
	//Rule: Category name must be provided
	codeBehind.GetLookupValues(string.Empty);
	codeBehind.ErrorDetails.Dump("Category name must be provided");

	//Rule: Category name must exist
	codeBehind.GetLookupValues("asdasdASD");
	codeBehind.ErrorDetails.Dump("Category name must exist");
	
	//Rule: No values found for the provided category name
	codeBehind.GetLookupValues("TestAAAA");
	codeBehind.ErrorDetails.Dump("No values found for the provided category name");
	
	//Pass: Valid category name with Values
	codeBehind.GetLookupValues("Province");
	codeBehind.Lookups.Dump("Pass - Valid category name with Values");
	
	//AddEditCustomer Tests
	//Add New Customer
	//Rule: Customer cannot be null
	codeBehind.AddEditCustomer(null);
	codeBehind.ErrorDetails.Dump("Customer cannot be null");
	
	//Setup new Customer
	CustomerEditView customerView = new();
	
	//rule: first name is required
	//rule: last name is required
	//rule: phone number is required
	//rule: email is required
	codeBehind.AddEditCustomer(customerView);
	codeBehind.ErrorDetails.Dump("required values not provided");

	//set up duplicate values
	customerView.FirstName = "Aiden";
	customerView.LastName = "Allen";
	customerView.Phone = "7809544417";
	customerView.Email = "test@test.ca";
	
	//rule: Customer first name, last name, and phone number must be unique
	codeBehind.AddEditCustomer(customerView);
	codeBehind.ErrorDetails.Dump("customer values were not unique");
	
	//Pass: Create New Customer
	//create good values
	customerView.FirstName = "Tina";
	customerView.LastName = "Caron";
	customerView.Phone = "780555555";
	codeBehind.AddEditCustomer(customerView);
	codeBehind.Customer.Dump("Pass - Valid customer added");
	

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

	public CustomerEditView Customer = default!;
	
	public List<LookupView> Lookups = default!;

	public void GetCustomer_ByID(int customerID)
	{
		// clear previous error details and messages
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = String.Empty;

		// wrap the service call in a try/catch to handle unexpected exceptions
		try
		{
			var result = CustomerService.GetCustomer_ByID(customerID);
			if (result.IsSuccess)
				Customer = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
		}
	}
	
	public void GetLookupValues(string categoryName)
	{
		// clear previous error details and messages
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = String.Empty;

		// wrap the service call in a try/catch to handle unexpected exceptions
		try
		{
			var result = CustomerService.GetLookupValues(categoryName);
			if (result.IsSuccess)
				Lookups = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
		}
	}
	
	public void AddEditCustomer(CustomerEditView editCustomer)
	{
		// clear previous error details and messages
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = String.Empty;

		// wrap the service call in a try/catch to handle unexpected exceptions
		try
		{
			var result = CustomerService.AddEditCustomer(editCustomer);
			if (result.IsSuccess)
				Customer = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
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
	public Result<CustomerEditView> GetCustomer_ByID(int customerID)
	{
		var result = new Result<CustomerEditView>();
		//rule: CustomerID must be valid
		if (customerID <= 0)
		{
			result.AddError(new Error("Missing Information", "Please provide a valid Customer ID."));
			return result;
		}


		var customer = _context.Customers
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

		if (customer == null)
		{
			result.AddError(new Error("No Customer", $"No customer was found with ID: {customerID}"));
			return result;
		}

		//return the result
		return result.WithValue(customer);
	}

	public Result<List<LookupView>> GetLookupValues(string categoryName)
	{
		var result = new Result<List<LookupView>>();
		//rule: categoryName must not be null or whitespace
		if (string.IsNullOrWhiteSpace(categoryName))
		{
			result.AddError(new Error("Missing Information", "Please provide a category name."));
			return result;
		}
		//rule: the Lookup category must exist
		if (!_context.Categories.Any(x => x.CategoryName.ToLower() == categoryName.ToLower()))
		{
			result.AddError(new Error("Invalid Category", $"{categoryName} is not a valid lookup category."));
			return result;
		}
		var values = _context.Lookups
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
		//If returning a list then check if the count is <= 0
		//If returning a single record, check if nothing was returned by looking for null
		if (values.Count <= 0)
		{
			result.AddError(new Error("No Lookup Values", $"No lookup values found for category {categoryName}."));
			return result;
		}
		//return the results with the value(s) from the database LINQ query 
		return result.WithValue(values);
	}

	public Result<CustomerEditView> AddEditCustomer(CustomerEditView editCustomer)
	{
		//Create a Result Container that will hold either a list of errors
		//or the successfully edited CustomerEditView
		var result = new Result<CustomerEditView>();

		//rule: editCustomer cannot be null
		if (editCustomer == null)
		{
			result.AddError(new Error("Missing Information", "No customer was provided"));
			//We need to exit the method because we have nothing to update or add
			return result;
		}

		//rule: first name, last name, phone number, email, country
		//are required (not empty)
		//We will collect all errors, to tell the user everything that is incorrect at once.
		if (string.IsNullOrWhiteSpace(editCustomer.FirstName))
			result.AddError(new Error("Missing Information", "First name is required"));
		if (string.IsNullOrWhiteSpace(editCustomer.LastName))
			result.AddError(new Error("Missing Information", "Last name is required"));
		if (string.IsNullOrWhiteSpace(editCustomer.Phone))
			result.AddError(new Error("Missing Information", "Phone number is required"));
		if (string.IsNullOrWhiteSpace(editCustomer.Email))
			result.AddError(new Error("Missing Information", "Email is required"));
		if(editCustomer.CountryID <= 0)
			result.AddError(new Error("Missing Information", "Country is required"));

		//rule: first name, last name, and phone number cannot be duplicated
		//(found more than once)
		//first: check if this is an add or edit
		//we check by seeing if the provided CustomerID is 0 or not
		//if it is not 0 then it is an edit and we don't need to check this.
		//if it is 0 that means the record doesn't exist in the database yet.
		if (editCustomer.CustomerID == 0)
		{
			//second: search the database for any matching records
			bool customerExists = _context.Customers
									.Any(x => x.FirstName.ToLower() == editCustomer.FirstName.ToLower()
											&& x.LastName.ToLower() == editCustomer.LastName.ToLower()
											&& x.Phone == editCustomer.Phone);
			//third: check if customerExists is true, if so a customer was found with the provided info
			if (customerExists)
				result.AddError(new Error("Existing Customer Data", $"A customer with the name {editCustomer.FirstName} {editCustomer.LastName} and the phone number {editCustomer.Phone} already exists in the database and cannot be entered again"));
		}

		//check if there are any errors at this point (before adding or editing the data)
		//and return the errors if any
		if (result.IsFailure)
			return result;

		//If no errors try and retrieve the customer from the database
		//This is not the ViewModel, this is the actual database record
		//we need to retrieve the data as the entity
		Customer customer = _context.Customers
								.Where(x => x.CustomerID == editCustomer.CustomerID)
								.FirstOrDefault();
		//check if the results are null
		//if null create a new customer record
		if (customer == null)
			customer = new Customer();

		//Edit the database entity record values or the new customer entity values
		//If edit, will update any data with the new provided data
		//if new, will add the new provided to the new record.
		customer.FirstName = editCustomer.FirstName;
		customer.LastName = editCustomer.LastName;
		customer.Address1 = editCustomer.Address1;
		customer.Address2 = editCustomer.Address2;
		customer.City = editCustomer.City;
		customer.ProvStateID = editCustomer.ProvStateID;
		customer.CountryID = editCustomer.CountryID;
		customer.PostalCode = editCustomer.PostalCode;
		customer.Phone = editCustomer.Phone;
		customer.Email = editCustomer.Email;
		customer.StatusID = editCustomer.StatusID;
		customer.RemoveFromViewFlag = editCustomer.RemoveFromViewFlag;

		//last check if new customer
		//remember this is only local changes!
		//nothing is saved to the database yet
		if (customer.CustomerID == 0)
			//if new add the record
			_context.Customers.Add(customer);
		else
			//if not update the record
			_context.Customers.Update(customer);
		try
		{
			//Save the changes to the database
			_context.SaveChanges();
			//Use the method you already coded
			//Make sure to use the database entity ID field for this
			//Why? If it was a new data entry, the editCustomer CustomerID will still be 0
			//But, the customer.CustomerID will have the new database assigned CustomerID
			//THIS IS A VERY VERY COMMON MISTAKE TO MAKE
			//NOW YOU KNOW :)
			return GetCustomer_ByID(customer.CustomerID);
		}
		catch (Exception ex)
		{
			//if something goes wrong is saving the changes
			//we MUST clear any staged changes to the database
			_context.ChangeTracker.Clear();
			result.AddError(new Error("Error Saving Changes", ex.InnerException.Message));
			return result;
		}
	}
}
	#endregion

	// ———— PART 4: View Models → Service Library View Model ————
	//	This region includes the view models used to 
//	represent and structure data for the UI.
#region View Models
public class CustomerEditView
{
	public int CustomerID { get; set; }
	//All strings should have a default of string.Empty
	public string FirstName { get; set; } = string.Empty;
	public string OrginalFirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string Address1 { get; set; } = string.Empty;
	public string Address2 { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public int ProvStateID { get; set; }
	public int CountryID { get; set; }
	public string PostalCode { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
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