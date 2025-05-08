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

//Ordering In-Class Exercise
//Question 1: Single Where Clause, Ordered by Last name
//The HireDate is Nullable (has the ? after the Data Type)
//When we see the ? after a datatype that is not normally nullable
//We can access the underlying or actual value with .Value
	// Note: Objects are nullable
	
//Creating a DateOnly using the Syntax:
//new DateOnly(yyyy, mm, dd)

//Ordering should ALWAYS be after the Filtering and 
//normally after the select when possible, because then you are
//working with the smaller dataset to order
	//Exceptions is when the ordering is on a field that is not selected.
	//However, you would still order after the filtering in this case.

Employees.Where(x => x.HireDate.Value >= new DateOnly(2022, 01, 01))
	//.Select(x => x) - .Select here is no longer needed, 
	//this just selects the whole record
	.OrderBy(x => x.LastName)
	.Dump();