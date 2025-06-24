using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.ViewModels
{
    public class CustomerSearchView
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CityProv { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
    }
}
