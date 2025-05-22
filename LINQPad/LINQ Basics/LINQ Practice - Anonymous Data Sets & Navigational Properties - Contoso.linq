<Query Kind="Statements">
  <Connection>
    <ID>374020ee-6f58-4b79-9e75-9188b7d91fb9</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>MOMSDESKTOP\SQLEXPRESS</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>Contoso</Database>
    <ExcludeRoutines>true</ExcludeRoutines>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

//Question 2
Customers
	.Where(x => x.Geography.StateProvinceName == "British Columbia"
			&& x.Geography.RegionCountryName == "Canada")
	.Select(x => new
	{
		FirstName = x.FirstName,
		LastName = x.LastName,
		CityName = x.Geography.CityName
	})
	.OrderBy(x => x.CityName)
	.ThenBy(x => x.LastName)
	.Dump("Question 2");
	
//Question 3
//Remember to include the brackets () around the OR clause or you end up asking the wrong question.
Products
	.Where(x => x.ProductSubcategory.ProductCategory.ProductCategoryName == "Audio"
			&& (x.ProductSubcategory.ProductSubcategoryName == "Recording Pen" 
				|| x.ProductSubcategory.ProductSubcategoryName == "Bluetooth Headphones")
			&& x.ColorName == "Pink")
	.Dump();