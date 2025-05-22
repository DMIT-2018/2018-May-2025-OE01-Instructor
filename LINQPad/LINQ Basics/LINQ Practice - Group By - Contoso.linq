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

//Group By Practice

//Question 1
//Without GroupBy
ProductCategories
	.Select(x => new {
		CategoryName = x.ProductCategoryName,
		ProductSubcategory = x.ProductSubcategories
								.Select(s => new {
									s.ProductSubcategoryName
								})
								.OrderBy(s => s.ProductSubcategoryName)
								.ToList()
	})
	.OrderBy(x => x.CategoryName)
	.ToList().Dump();

//With GroupBy
//This is not needed for this, because we can use the navigational properties.
ProductSubcategories
	.GroupBy(p => new
	{
		p.ProductCategory.ProductCategoryName
	})
	.OrderBy(p => p.Key.ProductCategoryName)
	.Select(g => new
	{
		CategoryName = g.Key,
		ProductSubcategories = g.Select(x => new
		{
			SubCategoryName = x.ProductSubcategoryName
		}).OrderBy(x => x.SubCategoryName)
	})
	.ToList()
	.Dump();