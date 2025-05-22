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

//Navigation and Anonymous Data Types

Albums
	.Where(x => x.AlbumId < 6)
	.Select(x => new
	{
		//Album is the new label for the field
			//How we need to reference it after this select
		//We can retrieve the record's data with our placeholder (x)
		Album = x.Title,
		Label = x.ReleaseLabel,
		Year = x.ReleaseYear,
		//Because Artist is the Parent of Album we can retrieve
		//data from the SINGLE Artist record associated with each album
		Artist = x.Artist.Name,
		//Because Tracks is a Child record, there is a potential for
		//MANY records and tracks is a collection
		//We cannot reference a single record from the collection and get
		//the name of a track
		//TrackName = x.Tracks.Name
		//However, we can navigate to Tracks from Album to get a collection
		//In order to use .Count()
		NumberOfTracks = x.Tracks.Count()
	})
	.Dump();
	
//Starting at Artist possible?
//Nope, Albums is a collection (child) of Artist which means we cannot
//access the individual AlbumIDs in the where clause
//Artists
//	.Where(x => x.Albums.AlbumID)

//Ternary Operator
Albums
	.Select(x => new
	{
		Title = x.Title,
		Artist = x.Artist.Name,
		Year = x.ReleaseYear,
		Label = x.ReleaseLabel == null ? "Unknown" : x.ReleaseLabel
	})
	.Dump();
	
//if(x.ReleaseYear == null)
//	"Unknown";
//else
//	x.ReleaseYear;

//Label the Decade of the Albums
Albums
	//We cannot reference Decade before it exists
	//It only exists after we define it with the SELECT
	//.OrderBy(x => x.Decade)
	.Select(x => new
	{
		Title = x.Title,
		Artist = x.Artist.Name,
		Year = x.ReleaseYear,
		Decade = x.ReleaseYear < 1970 ? "Oldies" :
					x.ReleaseYear < 1980 ? "70s" :
					x.ReleaseYear < 1990 ? "80s" :
					x.ReleaseYear < 2000 ? "90s" : "Modern"
	})
	//After the select statement we can reference the fields as
	//we defined them in the anonymous dataset
	//For example, we can now reference the field Decade
	.OrderBy(x => x.Decade)
	//We cannot order by anything after the select that no longer exists.
	//ReleaseYear is in the Data Class of Album
	//Not in the new Anonymous Dataset we created with the SELECT
	//.OrderBy(x => x.ReleaseYear)
	.Dump();