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
</Query>

void Main()
{
	GetCustomers("","444").Dump();
}

// You can define other methods, fields, classes and namespaces here
#region Test Methods
#endregion

#region Methods
public List<CustomerSearchView> GetCustomers(string lastName, string phone)
{
	//rule: Both last name and phone number cannot be empty
	if(string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(phone))
		throw new ArgumentNullException("Please provide either a last name and/or a phone number");
	
	//rule: RemoveFromViewFlag cannot be true
	return Customers
		.Where(x => !x.RemoveFromViewFlag 
						&&
				(
					(
						lastName != null 
							&& 
						x.LastName.ToLower().Contains(lastName.ToLower())
					) 
					|| 
					(
						phone != null 
							&& 
						x.Phone.Contains(phone)
					)
				)
			)
		.Select(x => new CustomerSearchView
		{
			CustomerID = x.CustomerID,
			FirstName = x.FirstName,
			LastName = x.LastName,
			City = x.City,
			Phone = x.Phone,
			Email = x.Email,
			StatusID = x.StatusID,
			Status = x.Status.Name,
			TotalSales = x.Invoices.Sum(i => i.SubTotal + i.Tax)
		})
		.ToList();
					
	
}
#endregion

#region Support Methods
public Exception GetInnerException(System.Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

#region ViewModels
public class CustomerSearchView
{
	public int CustomerID { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string City { get; set; }
	public string Phone { get; set; }
	public string Email { get; set; }
	public int StatusID { get; set; }
	public string Status { get; set; }
	public decimal TotalSales { get; set; }
}
#endregion