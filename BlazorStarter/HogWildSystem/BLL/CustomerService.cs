using BYSResults;
using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;

namespace HogWildSystem.BLL
{
    public class CustomerService
    {
        private readonly HogWildContext _context;

        internal CustomerService(HogWildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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
            if (editCustomer.CountryID <= 0)
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
            Customer? customer = _context.Customers
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
            customer.ProvStateID = editCustomer.ProvStateID ?? 0;
            customer.CountryID = editCustomer.CountryID ?? 0;
            customer.PostalCode = editCustomer.PostalCode;
            customer.Phone = editCustomer.Phone;
            customer.Email = editCustomer.Email;
            customer.StatusID = editCustomer.StatusID ?? 0;
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
                result.AddError(new Error("Error Saving Changes", ex.InnerException?.Message ?? ""));
                return result;
            }
        }
    }
}
