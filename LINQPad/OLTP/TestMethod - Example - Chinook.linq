<Query Kind="Program">
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

void Main()
{
	
}

// We will end up moving the Data CRUD methods to our service classes
// And the Test Methods are useful as the calls from our Blazor Pages.
#region Test Methods
//Make sure whatever method you are testing, you have the same return type
	//And the test method takes the exact same parameters
//Name it the same as the method being tested with the prefix Test
public AlbumEditView TestGetAlbum_ByID(int albumID) {
	//Whenever we call a Service method you MUST call that service method
	//within a try/catch
	//This saves kittens - I'm kidding, but it saves you a headache
	try 
	{
		
	}
	catch (Exception ex)
	{
		
	}
}
#endregion

#region Methods
//Method to provide an Album ID and return the Album Information
	//ID, Title, Artist Name, Release Year, Label, Count of Tracks
public AlbumEditView GetAlbum_ByID(int albumID)
{
	//rule: AlbumID must be valid - greater than 0
	if(albumID <= 0)
		throw new ArgumentNullException("Please provide a valid album ID. (greater than 0)");
		
	return Albums
		.Where(x => x.AlbumId == albumID)
		.Select(x => new AlbumEditView
		{
			AlbumID = x.AlbumId,
			Title = x.Title,
			ArtistName = x.Artist.Name,
			ReleaseYear = x.ReleaseYear,
			Label = x.ReleaseLabel,
			CountOfTracks = x.Tracks.Count()
		})
		.FirstOrDefault();
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
//Create the view models to transfer data that we want
//in the format that we need
public class AlbumEditView 
{
	public int AlbumID { get; set; }
	public string Title { get; set; }
	public string ArtistName { get; set; }
	public int ReleaseYear { get; set; }
	public string Label { get; set; }
	public int CountOfTracks { get; set; }
}
#endregion