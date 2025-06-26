using BYSResults;
using HogWildSystem.DAL;
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
    }
}
