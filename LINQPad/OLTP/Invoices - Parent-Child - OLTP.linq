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
	
	#region GetParts
	//Pass: Parts are returned
	codeBehind.GetParts();
	codeBehind.Parts.Dump("Pass - Parts returned");
	#endregion
	
	#region GetCustomerInvoices
	//Fail
	// Rule: customer ID must be greater than zero
	codeBehind.GetCustomerInvoices(0);
	codeBehind.ErrorDetails.Dump("Customer ID must be greater than zero");
	
	// Rule: customer ID must be valid
	codeBehind.GetCustomerInvoices(10000000);
	codeBehind.ErrorDetails.Dump("No invoices found for the supplied ID");
	
	// Pass: valid customer ID
	codeBehind.GetCustomerInvoices(1);
	codeBehind.CustomerInvoices.Dump("Pass - Valid Customer ID");
	#endregion
	
	#region GetInvoice
	//Fail
	// Rule: Customer ID and Employee ID must be greater than 0
	codeBehind.GetInvoice(0, 0, 0);
	codeBehind.ErrorDetails.Dump("Customer ID and Employee ID must be greater than 0");
	
	//Pass - New Invoice
	codeBehind.GetInvoice(0, 1, 1);
	codeBehind.Invoice.Dump("Pass - New Invoice");
	
	//Pass - Existing Invoice
	codeBehind.GetInvoice(31, 1, 1);
	codeBehind.Invoice.Dump("Pass - Existing Invoice");
	#endregion

	#region AddEditInvoice
	//Create an invoice and test the rules for an invoice
	//rule: invoiceView must be provided
	codeBehind.AddEditInvoice(null);
	codeBehind.ErrorDetails.Dump("Invoice cannot be null");
	
	//rule: customer id, employee id, and invoice lines must be provided
	//Setup a new invoice
	InvoiceView invoiceView = new();
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.ErrorDetails.Dump("Customer id, employee id, and invoice lines must be provided");
	
	//update to fix missing values
	invoiceView.CustomerID = 1;
	invoiceView.EmployeeID = 1;
	
	//create a child record
	InvoiceLineView invoiceLineView = new();
	//add the child to the parent record
	invoiceView.InvoiceLines.Add(invoiceLineView);
	//rule: for each invoice line, a part must be provided
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.ErrorDetails.Dump("For each invoice line, a part must be provided");
	
	//Make sure to give it a part so it doesn't keep failing on part
	invoiceView.InvoiceLines[0].PartID = 1;
	invoiceView.InvoiceLines[0].Price = -1;
	invoiceView.InvoiceLines[0].Quantity = 0;
	// rule: price is less than zero
	// rule: quantity is less than one
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.ErrorDetails.Dump("For each invoice line, Part Price >= 0 and Quantity > 0");
	
	//Fix the errors, cause we tested them
	invoiceView.InvoiceLines[0].Price = 15;
	invoiceView.InvoiceLines[0].Quantity = 1;
	
	//Create a duplicate part (part with the same PartID)
	invoiceLineView = new InvoiceLineView
	{
		PartID  = 1,
		Price = 10,
		Quantity = 4
	};
	//Add the new part to our invoice
	invoiceView.InvoiceLines.Add(invoiceLineView);

	// rule: duplicate parts added
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.ErrorDetails.Dump("Invoice Lines cannot be duplicated.");
	
	//Fix the errors, cause we tested it
	invoiceView.InvoiceLines[1].PartID = 2;
	//Make sure to add a date;
	invoiceView.InvoiceDate = DateOnly.FromDateTime(DateTime.Now);
	
	// Pass: valid new invoice
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.Invoice.Dump("Pass - New Invoice");
	
	//get the last invoice InvoiceID
	int invoiceID = Invoices.Max(x => x.InvoiceID);
	codeBehind.GetInvoice(invoiceID, 1, 1);
	//Update the invoice with new data
	invoiceView = codeBehind.Invoice;
	invoiceView.CustomerID = 2;
	invoiceView.EmployeeID = 3;
	
	//Update an existing LineItem with new data.
	invoiceView.InvoiceLines[0].Quantity = 22;
	//Add a new invoice Line
	invoiceLineView = new InvoiceLineView
	{
		PartID = 3,
		Price = 22,
		Quantity = 3
	};
	invoiceView.InvoiceLines.Add(invoiceLineView);
	// Pass: edit invoice
	codeBehind.AddEditInvoice(invoiceView);
	codeBehind.Invoice.Dump("Pass - Update Invoice");
	
	
	//Edit the created invoice
	
	#endregion

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
	private readonly PartService PartService = new PartService(context);
	#endregion

	#region Fields from Blazor Page Code-Behind
	// feedback message to display to the user.
	private string feedbackMessage = string.Empty;
	// collected error details.
	private List<string> errorDetails = new();
	// general error message.
	private string errorMessage = string.Empty;
	#endregion 
	
	public List<PartView> Parts = [];
	public List<InvoiceView> CustomerInvoices = [];
	public InvoiceView Invoice = default!;
	
	public void GetParts()
	{
		//Clear the feedback and error messages first
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = string.Empty;
		
		try
		{
			var result = PartService.GetParts();
			if(result.IsSuccess)
				Parts = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
			errorMessage.Dump();
		}
	}
	public void GetCustomerInvoices(int customerID)
	{
		//Clear the feedback and error messages first
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = string.Empty;

		try
		{
			var result = InvoiceService.GetCustomerInvoices(customerID);
			if (result.IsSuccess)
				CustomerInvoices = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
			errorMessage.Dump();
		}
	}
	public void GetInvoice(int invoiceID, int customerID, int employeeID)
	{
		//Clear the feedback and error messages first
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = string.Empty;

		try
		{
			var result = InvoiceService.GetInvoice(invoiceID,customerID,employeeID);
			if (result.IsSuccess)
				Invoice = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
			errorMessage.Dump();
		}
	}
	public void AddEditInvoice(InvoiceView invoiceView)
	{
		//Clear the feedback and error messages first
		errorDetails.Clear();
		errorMessage = string.Empty;
		feedbackMessage = string.Empty;

		try
		{
			var result = InvoiceService.AddEditInvoice(invoiceView);
			if (result.IsSuccess)
				Invoice = result.Value;
			else
				errorDetails = GetErrorMessages(result.Errors.ToList());
		}
		catch (Exception ex)
		{
			// capture any exception message for display
			errorMessage = ex.Message;
			errorMessage.Dump();
		}
	}

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
							InvoiceLines = x.InvoiceLines
											.Where(i => !i.RemoveFromViewFlag)
											.Select(i => new InvoiceLineView
											{
												InvoiceLineID = i.InvoiceLineID,
												InvoiceID = i.InvoiceID,
												PartID = i.PartID,
												Quantity = i.Quantity,
												Description = i.Part.Description,
												Price = i.Price,
												Taxable = i.Part.Taxable,
												RemoveFromViewFlag = i.RemoveFromViewFlag
											}).ToList(),
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
	
	public Result<InvoiceView> AddEditInvoice(InvoiceView invoiceView)
	{
		var result = new Result<InvoiceView>();
		
		#region Business Logic
		//rule: invoiceView must be provided
		if(invoiceView == null)
		{
			result.AddError(new Error("Missing Information", "Please provide an invoice"));
			//If they give us nothing or do not provide the parameters, just GTFO
			//There is no need to force your code to go through any additional logic
			return result;
		}
		//rule: customer id must be provided
		if(invoiceView.CustomerID == 0)
			result.AddError(new Error("Missing Information", "Please provide a valid customer ID"));
		//rule: employee id must be provided
		if (invoiceView.EmployeeID == 0)
			result.AddError(new Error("Missing Information", "Please provide a valid employee ID"));
		//rule: there must be invoice lines provided
		if(invoiceView.InvoiceLines.Count == 0)
			result.AddError(new Error("Missing Information", "Invoice details are required"));
			
		//Once we check the main record, we need to check the child records
		foreach(var invoiceLine in invoiceView.InvoiceLines)
		{
			//rule: for each invoice line, a part must be provided
			if(invoiceLine.PartID == 0)
			{
				result.AddError(new Error("Missing Information", "Missing Part ID"));
				//if no part was supplied, it time to GTFO
				//because we cannot proceed with checking the rest of the logic
				//Because we need to potentially tell the user which part has other errors.
				return result;
			}
			
			
			//rule: for each invoice line, the price must be greater than 0
			if(invoiceLine.Price < 0)
			{
				string partName = _context.Parts
									.Where(x => x.PartID == invoiceLine.PartID)
									.Select(x => x.Description)
									.FirstOrDefault();
				result.AddError(new Error("Invalid Price", $"Part {partName} has a price that is less than zero"));
			}
			//rule: for each invoice line, the quantity cannot be lkess than 1
			if (invoiceLine.Quantity < 1)
			{
				string partName = _context.Parts
									.Where(x => x.PartID == invoiceLine.PartID)
									.Select(x => x.Description)
									.FirstOrDefault();
				result.AddError(new Error("Invalid Quantity", $"Part {partName} has a quantity that is less than one"));
			}
				
		}

		//Make sure you are outside the foreach loop!
		// rule: parts cannot be duplicated on more than one line.
		List<string> duplicatedParts = invoiceView.InvoiceLines
											.GroupBy(x => new { x.PartID })
											.Where(x => x.Count() > 1)
											.OrderBy(x => x.Key.PartID)
											//No way to use a navigational property with a GroupBy like this
											//We need to go directly to Parts and use a .Where()
											.Select(x => _context.Parts
															.Where(p => p.PartID == x.Key.PartID)
															.Select(p => p.Description)
															.FirstOrDefault()
											).ToList();
											
		if(duplicatedParts.Count > 0)
		{
			foreach(var partName in duplicatedParts)
			{
				result.AddError(new Error("Duplicate Invoice Line Item", $"Part {partName} can only be added to the invoice once"));
			}
		}
		
		//exit if we have any errors
		if(result.IsFailure)
			return result;
		#endregion
		
		//Retrieve the actual database record (not as a ViewModel)
		Invoice invoice = _context.Invoices
							.Where(x => x.InvoiceID == invoiceView.InvoiceID)
							.FirstOrDefault();
		//If the invoice doesn't exist, create/initialize it.
		if(invoice == null)
		{
			invoice = new Invoice();
		}
		
		//Update the invoice properties from the supplied view model
		//Make sure to update any values that are not PKs or FKs
		invoice.CustomerID = invoiceView.CustomerID;
		invoice.EmployeeID = invoiceView.EmployeeID;
		invoice.InvoiceDate = invoiceView.InvoiceDate;
		//If it is flagged for deletion (logical delete) update the removefromviewflag
		invoice.RemoveFromViewFlag = invoiceView.RemoveFromViewFlag;
		
		//reset the subtotal and tax as we need to update this.
		invoice.SubTotal = 0;
		invoice.Tax = 0;
		
		//Process updating or adding each invoice line
		foreach(var invoiceLineView in invoiceView.InvoiceLines)
		{
			InvoiceLine invoiceLine = _context.InvoiceLines
										.Where(x => x.InvoiceLineID == invoiceLineView.InvoiceLineID
												&& !x.RemoveFromViewFlag)
										.FirstOrDefault();
			//If the line item doesn't exist or had been previous deleted, create it
			if(invoiceLine == null)
			{
				invoiceLine = new InvoiceLine();
				//Only updating the PartID if the record is new
				//if it exists, we should not change a FK record.
				invoiceLine.PartID = invoiceLineView.PartID;
			}
			//Update the properties from the view Model
			invoiceLine.Quantity = invoiceLineView.Quantity;
			invoiceLine.Price = invoiceLineView.Price;
			//If it flaged for deletion then this will update it.
			invoiceLine.RemoveFromViewFlag = invoiceLineView.RemoveFromViewFlag;
			
			//Check if it is new or existing
			//If it equals 0 it is new!
			if(invoiceLine.InvoiceLineID == 0)
				//Add new line item to the invoice entity (the parent record)
				//By using the navigational property and adding it to the parent record
				//When the parent record is saved, the FK (InvoiceID) of the parent record is automatically
				//	added to our new record.
				invoice.InvoiceLines.Add(invoiceLine);
			else
				//Since the PK is not 0, it is an update!
				//So we will STAGE (not saved to the database yet) the Update of the record.
				//	NOTE: DO NOT SAVE ANYTHING TO THE DATABASE YET
				_context.InvoiceLines.Update(invoiceLine);
				
			//If the record is not set to be removed (logical delete) we need
			//	to update the subtotal and tax
			if(!invoiceLineView.RemoveFromViewFlag)
			{
				invoice.SubTotal += invoiceLine.Quantity * invoiceLine.Price;
				bool isTaxable = _context.Parts
									.Where(x => x.PartID == invoiceLine.PartID)
									.Select(x => x.Taxable)
									.FirstOrDefault();
				invoice.Tax += isTaxable ? invoiceLine.Quantity * invoiceLine.Price * 0.05m : 0;
			}
		}
		
		//MAKE SURE YOU ARE OUTSIDE THE FOREACH LOOP
		//Check if the invoice is new or not
		if(invoice.InvoiceID == 0)
			//If it is 0 then STAGE add the invoice to the database
			_context.Invoices.Add(invoice);
		else
			//If it is not 0, it is an existing invoice, so STAGE the update
			_context.Invoices.Update(invoice);
		
		//Wrap any database changes in a TRY/CATCH always
		try 
		{
			//ACTUALLY save the changes to the database.
			_context.SaveChanges();
			//If the save doesn't have an error, return the invoice with the edits.
			//	We use the GetInvoice to make sure that the data is the same as saved.
			//	NOTE: Make sure you get the invoiceID from the database record
				//	If the invoice was NEW, the ViewModel record will still have an
				//	PK of 0, so you will end up returning a new record.
			return GetInvoice(invoice.InvoiceID, invoice.CustomerID, invoice.EmployeeID);
		}
		catch (Exception ex)
		{
			//Rollback any changes if there is an error
			//If you miss this step, then any other save attempts will
			//	still have your other database changes that already failed to save.
			//	P.S. You will keep getting errors.
			_context.ChangeTracker.Clear();
			result.AddError(new Error("Error Saving Changes",ex.InnerException.Message));
			return result;
		}
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

public class PartService
{
	#region Data Context Setup
	// The LINQPad auto-generated TypedDataContext instance used to query and manipulate data.
	private readonly TypedDataContext _context;

	// The TypedDataContext provided by LINQPad for database access.
	// Store the injected context for use in library methods
	// NOTE:  This constructor is simular to the constuctor in your service
	public PartService(TypedDataContext context)
	{
		_context = context
					?? throw new ArgumentNullException(nameof(context));
	}
	#endregion
	
	public Result<List<PartView>> GetParts()
	{
		var result = new Result<List<PartView>>();
		
		var parts = _context.Parts
						.Where(x => !x.RemoveFromViewFlag)
						.Select(p => new PartView
						{
							PartID = p.PartID,
							PartCategoryID = p.PartCategoryID,
							CategoryName = p.PartCategory.Name,
							Description = p.Description,
							Cost = p.Cost,
							Price = p.Price,
							ROL = p.ROL,
							QOH = p.QOH,
							Taxable = p.Taxable,
							RemoveFromViewFlag = p.RemoveFromViewFlag
						})
						.OrderBy(p => p.CategoryName)
						.ThenBy(p => p.Description)
						.ToList();
						
		if(parts == null || parts.Count == 0)
		{
			result.AddError(new Error("No Records Found", "No parts were found"));
			return result;
		}
		
		return result.WithValue(parts);
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
public class PartView
{
	public int PartID { get; set; }
	public int PartCategoryID { get; set; }
	public string CategoryName { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public decimal Cost { get; set; }
	public int ROL { get; set; }
	public int QOH { get; set; }
	public bool Taxable { get; set; }
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