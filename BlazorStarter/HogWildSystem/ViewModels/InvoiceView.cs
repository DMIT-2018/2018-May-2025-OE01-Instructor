using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.ViewModels
{
    public class InvoiceView
    {
        public int InvoiceID { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int CustomerID { get; set; }
        //Adding calculated fields (not math, just fields not in the Invoice Table in the Database)
        public string CustomerName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        //Read-only Field (get only), after the => (lamda) is what is returned if the field is called.
        public decimal Total => SubTotal + Tax;
        //When we are return a list of any child records WE MUST default it to an empty list ([])
        // [] means to default to a new empty collection (list, array, dictionary, etc.)
        public List<InvoiceLineView> InvoiceLines { get; set; } = [];
        public bool RemoveFromViewFlag { get; set; }
    }
}
