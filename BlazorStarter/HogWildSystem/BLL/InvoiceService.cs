using BYSResults;
using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class InvoiceService
    {
        private readonly HogWildContext _context;

        internal InvoiceService(HogWildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Result<List<InvoiceView>> GetInvoices_ByCustomerID(int customerID)
        {
            var result = new Result<List<InvoiceView>>();
            //rule: customer ID must be provided
            if (customerID == 0)
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
            if (customerInvoices == null || customerInvoices.Count == 0)
            {
                result.AddError(new Error("No Data Found", $"No invoices were found for customer ID {customerID}."));
                return result;
            }
            return result.WithValue(customerInvoices);
        }

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

        //Get or create a new InvoiceView (DO NOT SAVE TO THE DATABASE)
        public Result<InvoiceView> GetInvoice(int invoiceID, int customerID, int employeeID)
        {
            var result = new Result<InvoiceView>();

            //rule: both customerID and employeeID must be provided
            if (customerID == 0)
                result.AddError(new Error("Missing Information", "Please provide a customer ID"));
            if (employeeID == 0)
                result.AddError(new Error("Missing Information", "Please provide a employee ID"));
            if (result.IsFailure)
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
            if (invoiceView == null)
            {
                result.AddError(new Error("Missing Information", "Please provide an invoice"));
                //If they give us nothing or do not provide the parameters, just GTFO
                //There is no need to force your code to go through any additional logic
                return result;
            }
            //rule: customer id must be provided
            if (invoiceView.CustomerID == 0)
                result.AddError(new Error("Missing Information", "Please provide a valid customer ID"));
            //rule: employee id must be provided
            if (invoiceView.EmployeeID == 0)
                result.AddError(new Error("Missing Information", "Please provide a valid employee ID"));
            //rule: there must be invoice lines provided
            if (invoiceView.InvoiceLines.Count == 0)
                result.AddError(new Error("Missing Information", "Invoice details are required"));

            //Once we check the main record, we need to check the child records
            foreach (var invoiceLine in invoiceView.InvoiceLines)
            {
                //rule: for each invoice line, a part must be provided
                if (invoiceLine.PartID == 0)
                {
                    result.AddError(new Error("Missing Information", "Missing Part ID"));
                    //if no part was supplied, it time to GTFO
                    //because we cannot proceed with checking the rest of the logic
                    //Because we need to potentially tell the user which part has other errors.
                    return result;
                }


                //rule: for each invoice line, the price must be greater than 0
                if (invoiceLine.Price < 0)
                {
                    string partName = _context.Parts
                                        .Where(x => x.PartID == invoiceLine.PartID)
                                        .Select(x => x.Description)
                                        .FirstOrDefault() ?? "";
                    result.AddError(new Error("Invalid Price", $"Part {partName} has a price that is less than zero"));
                }
                //rule: for each invoice line, the quantity cannot be lkess than 1
                if (invoiceLine.Quantity < 1)
                {
                    string partName = _context.Parts
                                        .Where(x => x.PartID == invoiceLine.PartID)
                                        .Select(x => x.Description)
                                        .FirstOrDefault() ?? "";
                    result.AddError(new Error("Invalid Quantity", $"Part {partName} has a quantity that is less than one"));
                }

            }

            //Make sure you are outside the foreach loop!
            // rule: parts cannot be duplicated on more than one line.
            List<string?> duplicatedParts = invoiceView.InvoiceLines
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

            if (duplicatedParts.Count > 0)
            {
                foreach (var partName in duplicatedParts)
                {
                    result.AddError(new Error("Duplicate Invoice Line Item", $"Part {partName} can only be added to the invoice once"));
                }
            }

            //exit if we have any errors
            if (result.IsFailure)
                return result;
            #endregion

            //Retrieve the actual database record (not as a ViewModel)
            Invoice invoice = _context.Invoices
                                .Where(x => x.InvoiceID == invoiceView.InvoiceID)
                                .FirstOrDefault() ?? new();

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
            foreach (var invoiceLineView in invoiceView.InvoiceLines)
            {
                InvoiceLine? invoiceLine = _context.InvoiceLines
                                            .Where(x => x.InvoiceLineID == invoiceLineView.InvoiceLineID
                                                    && !x.RemoveFromViewFlag)
                                            .FirstOrDefault();
                //If the line item doesn't exist or had been previous deleted, create it
                if (invoiceLine == null)
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
                if (invoiceLine.InvoiceLineID == 0)
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
                if (!invoiceLineView.RemoveFromViewFlag)
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
            //If the invoice is not new (an update)
            //  Check if any lines were deleted
            if (invoice.InvoiceID != 0)
            {
                //Get a list of all previous lines from the database invoice record
                List<InvoiceLineView> referenceLines = _context.InvoiceLines
                    .Where(x => x.InvoiceID == invoice.InvoiceID
                            && x.RemoveFromViewFlag == false)
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
                    }).ToList();
                foreach(var line in referenceLines)
                {
                    //check and see if the invoiceline exists on the supplied invoice
                    if(!invoiceView.InvoiceLines.Any(x => x.InvoiceLineID == line.InvoiceLineID))
                    {
                        //Get the database record of the InvoiceLine
                        InvoiceLine? deletedInvoiceLine = _context.InvoiceLines
                            .Where(x => x.InvoiceLineID == line.InvoiceLineID)
                            .FirstOrDefault();

                        //Make sure we got it
                        if (deletedInvoiceLine != null)
                        {
                            //Update the RemoveFromViewFlag to true
                            deletedInvoiceLine.RemoveFromViewFlag = true;
                            //Update the database record
                            _context.InvoiceLines.Update(deletedInvoiceLine);
                        }
                    }
                }
            }
            //Check if the invoice is new or not
            if (invoice.InvoiceID == 0)
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
                result.AddError(new Error("Error Saving Changes", ex.InnerException?.Message ?? ""));
                return result;
            }
        }

        public Result<int> DeleteInvoice(int invoiceID)
        {
            var results = new Result<int>();
            //rule: Invoice ID must be provided
            if (invoiceID == 0)
            {
                results.AddError(new Error("Missing Information", "Please provide an invoice ID"));
                return results;
            }
                

            //Get the invoice from the database
            Invoice? invoice = _context.Invoices
                                    .Where(x => x.InvoiceID == invoiceID
                                            && !x.RemoveFromViewFlag)
                                    .FirstOrDefault();
            //rule: Invoice must exist and not already be removed
            if (invoice == null)
            {
                results.AddError(new Error("Missing Invoice", $"Invoice ID {invoiceID} does not exist in the system"));
                return results;
            }

            //Update the RemoveFromViewFlag
            invoice.RemoveFromViewFlag = true;
            _context.Invoices.Update(invoice);

            return results.WithValue(_context.SaveChanges());

        }
    }
}
