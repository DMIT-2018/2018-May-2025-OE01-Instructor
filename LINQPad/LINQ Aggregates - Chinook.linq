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

//Basic Aggregates
Albums
	.Select(x => new
	{
		Album = x.Title,
		Artist = x.Artist.Name,
		TrackCount = x.Tracks.Count(),
		//Remember an int divided by a int results in an int
		//Use m after the numbers to turn an int into a decimal to see
		//decimal results
		//Note: int / int Floors the results, it does not Round
			//Floor mean to drop any digits after the decimal
			//This result doesn't often give us enough information 
			//Example: is a track closer to 5 minutes or 6 minutes?
				//When floored we don't know
		MaxLength = x.Tracks.Max(x => x.Milliseconds) / 60000m,
		MinLength = x.Tracks.Min(x => x.Milliseconds) / 60000m,
		AvgLength = x.Tracks.Average(x => x.Milliseconds) / 60000,
		SumOfLengths = x.Tracks.Sum(x => x.Milliseconds) / 60000m
	}).Dump();

//Random info - Range Notation for strings!
var testString = "Testing a string thing and seeing if range notation works in LINQPad"[..20];
testString.Dump();

//Use Terinary operators to make sure there are as many characters as you want with Length
//If you don't you could get an OutOfRangeException
var testAgain = "test";
(testAgain.Length > 20 ? testAgain[..20] : testAgain).Dump();

//Use Range notation to trim the end of a String
//Example = remove a trailing comma
var testTrimEnd = "one, two, three, four,";

testTrimEnd[..^1].Dump();