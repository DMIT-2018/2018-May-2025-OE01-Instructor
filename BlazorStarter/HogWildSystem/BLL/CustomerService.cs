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
    }
}
