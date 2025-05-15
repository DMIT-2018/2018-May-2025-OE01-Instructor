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

//LINQ Odds and Ends
//First Examples
Albums
	.Where(x => x.AlbumId > 0)
	.First().Dump("First With Where");

Albums
	.First(x => x.AlbumId > 0)
	.Dump("First Without Where");
	
//With First if there is nothing found an exception is thrown
//Albums
//	.First(x => x.AlbumId == -1).Dump();

//Use FirstOrDefault to avoid the error, with objects we will return a null.
Albums
	.FirstOrDefault(x => x.AlbumId == -1)
	.Dump("Using FirstOrDefault, nothing found");

Albums
	.FirstOrDefault(x => x.AlbumId > 0)
	.Dump("Using FirstOrDefault");
	
//Single
Albums
	.Single(x => x.AlbumId == 1).Dump();

//If single find more than one result an error is thrown.
//Albums
//	.Single(x => x.AlbumId > 0).Dump();

//SingleOrDefault - Will still thrown an exception if more than one
//Albums
//	.SingleOrDefault(x => x.AlbumId > 0).Dump();

//SingleOrDefault will return the default (null) if it cannot find an item.
Albums
	.SingleOrDefault(x => x.AlbumId == 1000).Dump();
	
//Distinct - Returns the Distinct Record (1 copy of any duplicates)
Albums
	.Where(x => x.ReleaseYear > 1970)
	.OrderBy(x => x.ReleaseYear)
	.ThenBy(x => x.ReleaseLabel)
	.Select(x => new
	{
		Year = x.ReleaseYear,
		Label = x.ReleaseLabel
	}).Distinct().Dump();
	
//Take - Will take the first number of records specified.
//Can use OrderBy, Where, etc. with Take
Albums
	.OrderBy(x => x.Title)
	.Take(4).Dump();

Albums
	.OrderByDescending(x => x.Title)
	.Take(4).Dump();

//Find the 5 Longest Albums
Albums
	.OrderByDescending(x => x.Tracks.Sum(t => t.Milliseconds))
	.Take(5).Dump();
	
//Skip - Skips over the specified number of records
//Can be combined with Take
Albums
	.Skip(30).Take(5).Dump();

//Any - Very useful to return true or false if any records
//Super useful for if statements, for example
	//checking if a query returned any resulting records.
Albums
	.Any(x => x.ReleaseYear == 1975).Dump();
	
Albums
	.Any(x => x.ReleaseYear == 1400).Dump();

//Example check if any artists have 0 Albums
Artists
	.Select(x => new
	{
		Artist = x.Name,
		Count = x.Albums.Count()
	})
	.OrderBy(x => x.Count)
	.Dump();

//Can use Any to only return artists that have Albums.
Artists
	.Where(x => x.Albums.Any())
	.Dump();

//Can use the Any filter (where) to check and return
//Only artists that have Albums with a runtime over 5 minutes.
Artists
	.Where(x => x.Albums.Any(a => (a.Tracks.Sum(t => t.Milliseconds) /60000m) > 5))
	.Dump();

//All - Return true if all records meet the condition, false otherwise
Albums
	.All(x => x.ReleaseYear > 1400).Dump();

Albums
	.All(x => x.ReleaseYear > 1970).Dump();
	
//Combining Lists
List<int> numbersA = [1,2,3,4];
List<int> numbersB = [3,4,5,6];

//Union - combine lists by return the distinct values
numbersA.Union(numbersB).Dump("Union Results");

//Intersect - combine lists and return only what is present in both lists.
numbersA.Intersect(numbersB).Dump("Intersect Results");

//Except - Will return the elements from the First List that are not present in the second list
//Return everything from numbersA, except what is in numbersB
numbersA.Except(numbersB).Dump("Except Results");

//Concat - Return everything from both lists
numbersA.Concat(numbersB).Dump("Concat Results");

