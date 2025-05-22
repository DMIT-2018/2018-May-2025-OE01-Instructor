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
	#region GetArtist_ByID
	//Valid Data Tests
	Console.WriteLine("====================");
	Console.WriteLine("---- Get Artist By ID - Pass ----");
	Console.WriteLine("====================");
	TestGetAlbum_ByID(1).Dump("Pass - Valid ID - Album Found");
	TestGetAlbum_ByID(1000).Dump("Pass - Valid ID - No Album Found");
	Console.WriteLine("====================");
	Console.WriteLine("---- Get Artist By ID - Fail ----");
	Console.WriteLine("====================");
	//rule: Album ID must be > 0
	//Remember always that 0 is a special case, must always test is separately from
		//positive or negative numbers
	TestGetAlbum_ByID(0).Dump("Failure - ArtistID must be > 0 - 0 Test");
	TestGetAlbum_ByID(-12).Dump("Failure - ArtistID must be > 0 - Negative Test");x
	#endregion
	
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
		return GetAlbum_ByID(albumID);
	}
	//Catch the potential exceptions
	catch(ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	//We always have a backup of catch(Exception)
	//This is the generic catch in case something
	//Unexpected goes wrong
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	return null; //Ensures we always have a valid return
				 //Even when there are exceptions thrown
}
public List<AlbumEditView> TestGetAlbums_ByPartialTitle(string partialTitle) 
{
	try
	{
		return GetAlbums_ByPartialTitle(partialTitle);
	}
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (ArgumentException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	return null;
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
//Method to return all albums that contain the provided string in the title
public List<AlbumEditView> GetAlbums_ByPartialTitle(string partialTitle)
{
	//rule: a partial search string must be provided
	if(string.IsNullOrWhiteSpace(partialTitle))
		throw new ArgumentNullException("Partial search string is required.");
	//rule: must provide as least three characters to search by
	if(partialTitle.Length < 3)
		throw new ArgumentException("Partial search string must be at least 3 characters long.");
		
	return Albums
				//Make sure for partial string searched you have everything 
				//In upper or lower case (use the same)
				.Where(x => x.Title.ToLower().Contains(partialTitle.ToLower()))
				.Select(x => new AlbumEditView
				{
					AlbumID = x.AlbumId,
					Title = x.Title,
					ArtistName = x.Artist.Name,
					ReleaseYear = x.ReleaseYear,
					Label = x.ReleaseLabel,
					CountOfTracks = x.Tracks.Count()
				})
				.OrderBy(x => x.Title)
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