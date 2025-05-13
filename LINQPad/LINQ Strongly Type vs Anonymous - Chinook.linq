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
	IEnumerable anonymousResults = SongByPartialNameAnonymous("Dance");
	anonymousResults.Dump();
	
	//foreach(var test in anonymousResults)
	//{
	//	test.Dump();
	//	//Cannot access individual Field Values in an Anonymous Data Type
	//	//test.SongTitle.Dump();
	//}
	
	//Very Basic Collection type, cannot perform normal (expected) methods
	//such as Count
	//anonymousResults.Count().Dump();
	
	//Strongly Typed Examples
	List<SongView> results = SongByPartialName("Dance");
	results.Dump();
	
	results.Count().Dump();
	
	foreach(var strongTest in results)
	{
		strongTest.SongTitle.Dump();
	}
	
}

// You can define other methods, fields, classes and namespaces here
public IEnumerable SongByPartialNameAnonymous(string partialSongName)
{
	return Tracks
		.Where(x => x.Name.ToLower().Contains(partialSongName.ToLower()))
		.Select(x => new
		{
			AlbumTitle = x.Album.Title,
			SongTitle = x.Name,
			Artist = x.Album.Artist.Name
		})
		.OrderBy(x => x.SongTitle);
}

//When returning Strongly Typed (defined) results
//You set the return type to the defined type you are using
	//Put the type in between the <>
//Once the data is not on the server and we want to store in 
//memory, we should change to a List Collection
//Lists have many more options and are a less primitive collection
public List<SongView> SongByPartialName(string partialSongName)
{
	return Tracks
		.Where(x => x.Name.ToLower().Contains(partialSongName.ToLower()))
		//Instead of new, we need to say what the type is we are creating
		.Select(x => new SongView
		{
			//Can only include Fields that are defined in
			//the class we are creating
			AlbumTitle = x.Album.Title,
			SongTitle = x.Name,
			Artist = x.Album.Artist.Name
		})
		.OrderBy(x => x.SongTitle)
		//To return a list the absolutely last thing we do is call
		//ToList()
		//This must be last thing you do or it isn't a list.
		.ToList();
}

//We can create a Strongly Typed Class to Hold the Song Data
public class SongView 
{
	public string AlbumTitle { get; set; }	
	public string SongTitle { get; set; }	
	public string Artist { get; set; }	
}