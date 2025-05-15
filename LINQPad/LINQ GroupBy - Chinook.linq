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

Albums
	.GroupBy(x => x.ReleaseYear)
	.Select(x => new 
	{
		//Key is what the group category is, we can retrieve the value
		Year = x.Key,
		//We can retrieve the Grouped records and output them as a list.
		Albums = x.ToList(),
		//Once we are after the GroupBy we only have collections of data
			//Like Child Tables, we need to do a nested Query is order to
			//Return specific values.
		AlbumBasics = x.Select(a => new
						{
							Title = a.Title,
							Artist = a.Artist.Name,
							Year = x.Key,
							TrackCount = a.Tracks.Count()
						})
	}).Dump();
	
//What if we want to group by Artist and display the Artist Name?
Albums
	.GroupBy(x => x.ArtistId)
	.Select(x => new
	{
		//To retrieve a value from another table we CAN use a query and where clause
		Artist = Artists.Where(a => a.ArtistId == x.Key).FirstOrDefault().Name,
		Albums = x.ToList()
	}).Dump();

//We can also use navigational properties to Group By which will give us the name
//The issue might be if two artists have the same name they will be grouped together
	//Make sure the values have to be unique before you use this method.
Albums
	.GroupBy(x => x.Artist.Name)
	.Select(x => new
	{
		Artist = x.Key,
		Albums = x.ToList()
	}).Dump();
	
//The above is a bad example of using a group by because if we want
//a direct field from Artist and to see a list of Albums 
//we should just start at the Artist Table
Artists
	.Select(x => new
	{
		Artist = x.Name,
		Albums = x.Albums.ToList()
	}).Dump();
	
//REMEMBER: If you are grouping by a FK, you likely do not need a GroupBy
	
//What if the group key is null?
//We can use Terinary operators to check for nulls and replace the value
Albums
	.GroupBy(x => x.ReleaseLabel)
	.Select(x => new 
	{
		Label = x.Key == null ? "Unknown" : x.Key,
		Albums = x.ToList(),
		//We can use Aggregates because the results of a GroupBy are a collection
		AlbumCount = x.Count(),
		//We can also nest Aggregates to get the Sum of the Tracks UnitPrice 
		//for all the Albums
		TotalAlbumCost = x.Sum(a => a.Tracks.Sum(t => t.UnitPrice))
	})
	.OrderByDescending(x => x.AlbumCount)
	.Dump();
	
//What if you want to Group By multiple things?
Albums
	//GroupBy more than one field by defining an anonymous data type
		//Use the new keyword
	.GroupBy(x => new { x.ReleaseLabel, x.ReleaseYear })
	.Select(x => new 
	{
		//Reference each field in the Anonymous Group By data
			//Use x.Key.[FieldName]
		Label = x.Key.ReleaseLabel,
		Year = x.Key.ReleaseYear,
		Albums = x.ToList()
	})
	.OrderBy(x => x.Year)
	.ThenBy(x => x.Label)
	.Dump();

//We can Group By with multiple and use Navigational Properties
//What if we want to Group By Artist and Year
Albums
	.GroupBy(x => new 	{ 
							Artist = x.Artist.Name, 
							x.ReleaseYear 
						})
	.Select(x => new
	{
		Year = x.Key.ReleaseYear,
		Artist = x.Key.Artist,
		Albums = x.ToList()
	})
	.OrderBy(x => x.Year)
	.ThenBy(x => x.Artist)
	.Dump();