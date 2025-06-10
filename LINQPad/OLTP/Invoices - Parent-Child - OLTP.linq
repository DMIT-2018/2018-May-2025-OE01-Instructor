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
	private readonly InvoiceService InvoiceService = new InvoiceService(context);
	#endregion

	#region Fields from Blazor Page Code-Behind
	// feedback message to display to the user.
	private string feedbackMessage = string.Empty;
	// collected error details.
	private List<string> errorDetails = new();
	// general error message.
	private string errorMessage = string.Empty;
	#endregion

}
#endregion

// ———— PART 3: Database Interaction Method → Service Library Method ————
//	This region contains support methods for testing
#region Methods
public class InvoiceService
{
	#region Data Context Setup
	// The LINQPad auto-generated TypedDataContext instance used to query and manipulate data.
	private readonly TypedDataContext _context;

	// The TypedDataContext provided by LINQPad for database access.
	// Store the injected context for use in library methods
	// NOTE:  This constructor is simular to the constuctor in your service
	public InvoiceService(TypedDataContext context)
	{
		_context = context
					?? throw new ArgumentNullException(nameof(context));
	}
	#endregion

	public Result<List<InvoiceView>> GetCustomerInvoices(int customerID)
	{
		var result = new Result<List<InvoiceView>>();
		//rule: customer ID must be provided
		if(customerID == 0)
		{
			result.AddError(new Error("Missing Information", "Please provide a valid customer id."));
			return result;
		}
		
		//query
		//rule: remove from view flag is false
		var customerInvoices = _context.Invoices
								.Where(x => x.CustomerID == customerID
											&& !x.RemoveFromViewFlag)
								.Select(x => new InvoiceView 
								{
									InvoiceID = x.InvoiceID,
									CustomerID = x.CustomerID,
									CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
									EmployeeID = x.EmployeeID,
									EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
									InvoiceDate = x.InvoiceDate,
									SubTotal = x.SubTotal,
									Tax = x.Tax,
									RemoveFromViewFlag = x.RemoveFromViewFlag
								}).ToList();
		if(customerInvoices == null || customerInvoices.Count == 0)
		{
			result.AddError(new Error("No Data Found", $"No invoices were found for customer ID {customerID}."));
			return result;
		}
		return result.WithValue(customerInvoices);
	}
	
	//Get or create a new InvoiceView (DO NOT SAVE TO THE DATABASE)
	public Result<InvoiceView> GetInvoice(int invoiceID, int customerID, int employeeID)
	{
		var result = new Result<InvoiceView>();
		
		//rule: both customerID and employeeID must be provided
		if(customerID == 0)
			result.AddError(new Error("Missing Information", "Please provide a customer ID"));
		if (employeeID == 0)
			result.AddError(new Error("Missing Information", "Please provide a employee ID"));
		if(result.IsFailure)
			return result;

		// Handle both new and existing invoices
		// For a new invoice the following info is needed
		//CustomerID and EmployeeID
		//For an existing invoice the following info is needed
		//InvoiceID and EmployeeID
		//	We want the employeeID to ensure we have the current employee who is handling the transaction.
		InvoiceView invoice = default!;
		//Check if invoiceID is 0, if 0 then a new invoice is needed.
		if (invoiceID == 0)
		{
			invoice = new InvoiceView
			{
				CustomerID = customerID,
				EmployeeID = employeeID,
				InvoiceDate = DateOnly.FromDateTime(DateTime.Now)
			};
		}
		else 
		{
			invoice = _context.Invoices
						.Where(x => x.InvoiceID == invoiceID
								&& !x.RemoveFromViewFlag)
						.Select(x => new InvoiceView
						{
							InvoiceID = x.InvoiceID,
							CustomerID = x.CustomerID,
							EmployeeID = x.EmployeeID,
							InvoiceDate = x.InvoiceDate,
							SubTotal = x.SubTotal,
							Tax = x.Tax,
							//Add in nested Query to return InvoiceLines
							RemoveFromViewFlag = x.RemoveFromViewFlag
						}).FirstOrDefault();
			//set the customerID to the customer from the Method call
			customerID = invoice.CustomerID;
		}
		//Now we need to populate the Customer Name and Employee Name for display
		// Do this outside the if/else because it is needed for both new and existing invoices.
		invoice.CustomerName = GetFullCustomerName(customerID);
		invoice.EmployeeName = GetFullEmployeeName(employeeID);
		
		//happens if invoiceID doesn't exist in the database or invoice is marked as removed.
		if (invoice == null)
		{
			result.AddError(new Error("No results", $"No invoice was found for invoice ID {invoiceID}"));
			return result;
		}
		//NOTE: NOTHING WAS ADDED OR SAVED TO THE DATABASE
		return result.WithValue(invoice);
	}
	
	//Used by other methods in the Service and not by the front end (web pages) so these can be made Private
	private string GetFullCustomerName(int customerID)
	{
		
		return _context.Customers
				.Where(x => x.CustomerID == customerID)
				.Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault() ?? string.Empty;
	}

	private string GetFullEmployeeName(int employeeID)
	{

		return _context.Employees
				.Where(x => x.EmployeeID == employeeID)
				.Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault() ?? string.Empty;
	}
}
#endregion

// ———— PART 4: View Models → Service Library View Model ————
//	This region includes the view models used to 
//	represent and structure data for the UI.
#region View Models
public class InvoiceView 
{
	public int InvoiceID { get; set; }
	public DateOnly InvoiceDate { get; set; }
	public int CustomerID { get; set; }
	//Adding calculated fields (not math, just fields not in the Invoice Table in the Database)
	public string CustomerName { get; set; }
	public int EmployeeID { get; set; }
	public string EmployeeName { get; set; }
	public decimal SubTotal { get; set; }
	public decimal Tax { get; set; }
	//Read-only Field (get only), after the => (lamda) is what is returned if the field is called.
	public decimal Total => SubTotal + Tax;
	//When we are return a list of any child records WE MUST default it to an empty list ([])
		// [] means to default to a new empty collection (list, array, dictionary, etc.)
	public List<InvoiceLineView> InvoiceLines { get; set; } = [];
	public bool RemoveFromViewFlag { get; set; }
}
public class InvoiceLineView
{
	public int InvoiceLineID { get; set; }
	public int InvoiceID { get; set; }
	public int PartID { get; set; }
	//Calculated Field
	public string Description { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
	public bool Taxable { get; set; }
	//Read-Only Field
	public decimal ExtentPrice => Price * Quantity;
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