<Query Kind="Statements">
  <Connection>
    <ID>a4723aa7-0e0f-4e1a-a5e5-93d629592c0a</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>MOMSDESKTOP\SQLEXPRESS</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>ChinookSept2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

//x is the reference to each item in the collection
Albums.Select(x => x.Title).Dump();

//Foreach Loop Example
//x is the reference to each item in the collection
//foreach(var x in Albums)
//{
//	x.Title.Dump();
//}

//WHERE Clause Examples
Albums.Where(x => x.ReleaseLabel.StartsWith("A"))
		.Select(x => x.Title).Dump();
//Where with multiple requirements
//We do not use multiple Where Clauses, we use && for AND and || for OR
Albums.Where(x => x.ReleaseLabel.StartsWith("A") && x.Title.StartsWith("A"))
		.Select(x => x.Title).Dump();
//Remember to use Brackets, same as SQL when needed
Albums.Where(x => (x.ReleaseLabel.StartsWith("A") && x.Title.StartsWith("A")) || (x.ReleaseLabel.StartsWith("P") && x.Title.StartsWith("P")))
		.Select(x => x.Title).Dump();
		
//ORDERING/SORTING Examples
//Ordering Ascending
Albums.Where(x => x.ReleaseYear >= 1990 && x.ReleaseYear < 2000)
	.OrderBy(x => x.ReleaseYear)
	.ThenBy(x => x.Title)
	.Dump();

//Ordering Descending
Albums.Where(x => x.ReleaseYear >= 1990 && x.ReleaseYear < 2000)
	.OrderByDescending(x => x.ReleaseYear)
	.ThenByDescending(x => x.Title)
	.Dump();

//Ordering Mixed
Albums.Where(x => x.ReleaseYear >= 1990 && x.ReleaseYear < 2000)
	.OrderByDescending(x => x.ReleaseYear)
	.ThenBy(x => x.Title)
	.Dump();
	
