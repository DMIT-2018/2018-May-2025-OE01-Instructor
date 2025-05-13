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

Artists
	.Select(x => new
	{
		Artist = x.Name,
		//We cannot access individual Fields Values from Albums
		//Because Albums is a child record of Artist and is a 
		//Collection
		//AlbumName = x.Albums.Title <-- Not Possible
		//We can nest (sub-select) data from a collection with LINQ
		//When we sub-select from a collection of the main record
		//Make sure we use a different placehold 
		//(Ex: x for Artist, a for Album)
		//We can select more that a single value by Selecting to a 
		//anonymous dataset (we can also select as Strongly Type - See Below)
		//Note: You need to use Navigation to filter the list automatically
			//It's like an implied Where clause.
			//We only see the Albums associated with the current
			//Artist Record by using the navigational property
			//We want to use this navigation because it is faster (think
			//in millions of records) then a Where Clause because it
			//can use the PK and FK relationship from the database.
		//CAUTION: IF YOU USE A WHERE CLAUSE AND DO NOT USE NAVIGATIONAL
		//PROPERTIES FOR YOU ASSESSMENTS OR ASSIGNMENTS YOU WILL LOSE MARKS
		Albums = x.Albums.Select(a => new 
					{
						Album = a.Title,
						Label = a.ReleaseLabel,
						Year = a.ReleaseYear
					})
					.OrderBy(a => a.Album)
					.ToList()
	}).Dump("Anonymous Results");

Artists
	.Select(x => new
	{
		Artist = x.Name,
		Albums = x.Albums
					.Select(a => new
					{
						Album = a.Title,
						Label = a.ReleaseLabel,
						Year = a.ReleaseYear
					})
					.OrderBy(a => a.Album)
					.ToList()
	}).Dump();