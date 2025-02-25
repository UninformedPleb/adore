# ADORE

ADORE is a backronym for ADO is Really Easy. (Yes, it's kinda cornball.)

## Why use ADORE?

Many developers (myself among them) find low-level ADO to be tedious in its
out-of-the-box form. You have to manage every connection, every command, every
data adapter, and you have to make sure you clean up after it all. Nothing is
automatic, and everything has some cleanup task or another that needs to be
tended to.

ADORE simplifies ADO.NET code down to its logical operations: configure,
connect, query, get results. No more messing around with making sure the
connection state is valid or worrying about whether you remembered to dispose
everything afterward.

## A Little History - Why Did I Make ADORE?

I started ADORE many years ago, developed its concepts in various professional
projects through a couple of decades, and finally gave it a name when I uploaded
it to Github and Nuget. I got it to a semi-tested state, then let it languish
for years. I counted it as unnecessary and expected it had been supplanted by
Dapper.

But as I used Dapper more and more in my professional duties, I kept wishing for
features that I had built into ADORE. Mapping query results into domain objects
is great, but I still had to manually create a connection. And especially since
the launch of .NET Core, any simplicity that ADO.NET might have had is gone.
Cross-platform concerns had dismantled things like DbProviderFactories, and few
if any engineering efforts had been made to put it back.

## How to use ADORE

### Getting Started

#### Configure Providers

The first step in any project that uses ADORE, or really ADO.NET, is to
establish the database providers. They're the part that will handle each DBMS
according to its quirks.

For modern .NET, you're responsible for the registration of database providers.
These "providers" are the database access assemblies, like Npgsql or
Microsoft.Data.SqlClient. This part of ADORE is configuration-driven. Simply add
the providers to your all-environments config.

```JSON
"ADORE": {
	"ProviderFactories": [
		{
			"ProviderName": "MSSQLServer",
			"FactoryTypeName": "Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient"
		},
		{
			"ProviderName": "Postgres",
			"FactoryTypeName": "Npgsql.NpgsqlFactory, Npgsql"
		}
	]
}
```

ADORE will automatically register the providers in ADORE:ProviderFactories when
you add it to your services collection.

#### Configure Connection Strings

With the providers established, now it's time to configure the connections that
use those providers. This should be added to your per-environment config.

```JSON
"ADORE": {
	"ConnectionStrings": [
		{
			"ConnectionName": "musiclibrary",
			"ProviderName": "MSSQLServer",
			"ConnectionStringValues": {
				"Server": "HOSTNAME_OR_IP_ADDRESS",
				"Database": "DATABASE_CATALOG_NAME",
				"User ID": "USERNAME",
				"Encrypt": "Optional",
				"TrustServerCertificate": "True"
			}
		}
	]
}
```

And your secrets file or secure keystore should have:

```JSON
"ADORE": {
	"ConnectionStrings": {
		"ConnectionStringValues": {
			"Password": YOUR_PASSWORD_HERE
		}
	}
}
```

#### Configure ADORE

Next, ADORE needs to register everything. This one step will take care of all of
the provider factories, the connection strings, and any other configuration
values that are needed to persist ADORE throughout the lifetime of your app. It
will even register the AdoreConfig into the DI system.

```C#
builder.ConfigureAdore(builder.Configuration.GetSection("ADORE").Get<AdoreConfig>());
```

With this, all configured connections are available to be mapped to a Database
class object.

#### Register Databases

Each of the classes you make to correspond to a database (catalog) should be
set-up in DI. It will look something like this:

```C#
builder.Services.RegisterDatabase<MusicLibraryDatabase>("musiclibrary");
```

### Using ADORE

You can use ADORE without fully integrating it. There's an AdHocDatabase that
can provide basic one-off access to everything you need to run a quick query.
But ADORE is designed to be much more than that. If you use its features to the
full, it can make your database code seamlessly idiomatic with both SQL and
.NET.

#### Integrating ADORE Into Your Codebase

To use ADORE as it is intended, you'll need to implement your database structure
as implementations of the Database, Schema, and Query classes.

##### Implementing a Table

Tables are POCOs. There's nothing special about them, really. They serve as a
pre-configured mapping of a single database record. There's no base classes to
inherit or magic attributes to decorate everything with. Just POCOs.

Here are some examples:

```C#
public class Song
{
	public int SongID { get; set; }
	public int AlbumID { get; set; }
	public int GenreID { get; set; }
	public string Name { get; set; }
}

public class Artist
{
	public int ArtistID { get; set; }
	public string Name { get; set; }
}

public class Album
{
	public int AlbumID { get; set; }
	public string Name { get; set; }
	public string Publisher { get; set; }
	public byte[] CoverArtwork { get; set; }
}
```

Alternatively, you could use a record type instead:

```C#
public record Performance(int SongID, int ArtistID, DateTime PerformanceDate, string Instrument);
public record Genre(int GenreID, string Name);
```

The class would allow you to add methods to it if you wanted to, while that
wouldn't be possible with the record. But the record enforces immutability and
provides simple equality semantics. Which you choose is up to you.

##### Implementing a Schema

A schema groups database objects (tables, views, stored procedures, functions,
and other things) under a name and allows for security settings to be applied
as a group. But since these security settings aren't part of our application's
code, schemas are easy to treat like a namespace.

To represent a database schema, ADORE provides the Schema base class for you to
extend like this:

```C#
public class MetadataSchema : Schema
{
	public MetadataSchema(Database parent) : base(parent) { }
	public override string Name { get => "metadata"; }

	public async Genre LoadGenre(int id)
	{
		var proc = CreateStoredProcedure("Genre_Load", new { GenreID = id });
		return (await proc.Run<Genre>()).First();
	}
	public async Genre SaveGenre(Genre genre)
	{
		var proc = CreateStoredProcedure("Genre_Save", genre);
		return (await proc.Run<Genre>()).First();
	}
}
```

First is the constructor. The main purpose is to attach this instance to its
parent Database.

Next comes the Name property. The base class expects the getter to be
implemented so it can include the schema scope when it's needed. But you can
always add a different schema's procedure to a schema class if you want.

Next are the methods to call stored procedures. This optional syntax makes
data access code clear and idiomatic.

##### Implementing a Database

Now that we have tables and schemas, we can put them all into an object that
represents the database.

```C#
public class MusicLibraryDatabase : Database
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public DboSchema dbo { get; private set; }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public MetadataSchema metadata { get; private set; }

	public MusicLibraryDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString)
	{
		this.dbo = new DboSchema(this);
		this.metadata = new MetadataSchema(this);
	}
}
```

First, the database defines members for each of its schemas. These can be
flagged with the SuppressMessageAttribute, as seen above, if you use strict
naming convention rules. Of course, there's no requirement that these be
named exactly as they are in the database. It's just a suggestion.

Next is the constructor. It anchors our database object to its provider factory
and connection string. It also initializes the schema instances.

With that, the database class contains a ready-to-use instance of each of its
schemas and is complete.

##### Putting It All Together

To use the database via DI, put it into the constructor of your DI-participating
classes. Retrieving data is as simple as calling the methods you've added to the
schema.

The example below shows how the Genre object can be retrieved from LoadGenre(),
then updated in-place, then saved back to the database with SaveGenre().

```C#
public class FooThingy
{
	private ILogger _logger;
	private MusicLibraryDatabase _mldb;

	public FooThingy(ILogger logger, MusicLibraryDatabase mldb)
	{
		_logger = logger;
		_mldb = mldb;
	}

	public void DoStuff()
	{
		var genre = _mldb.metadata.LoadGenre(27); // magic number is magic... it's an example, okay?
		genre.Name = "Blah";
		genre = _mldb.metadata.SaveGenre(genre);
		_logger.LogInformation($"did stuff. genre: {genre.GenreID}, {genre.Name}");
	}
}
```

By setting things up this way, the semantics match the database structure. It's
also easily templated or generated.

#### Unintegrated Use

Q: But what if you just need to run a query and don't want so much formal code?

A: Use the AdHocDatabase. AdHocDatabase is a pre-made class that exposes the
basic Database methods CreateQuery and CreateStoredProcedure.

Here's how to use ADORE with all of the same features, but without the overhead
of maintaining a bunch of code structure. The following example does the same
thing as before, but without custom database and schema classes and without
mirroring a pre-defined database structure. It just needs some configuration.

```C#
public class FooThingy(ConnectionLoader cl)
{
	// NOTE: the ConnectionLoader is initialized with the DI setup shown earlier.

	public void DoStuff()
	{
		var adhoc = new AdHocDatabase(cl.GetFactory("musiclibrary"), cl.GetDatabaseConnection("musiclibrary"));
		var query = adhoc.GetStoredProcedure("metadata.Genre_Load", new { GenreID = 27 });
		var genre = query.Run<Genre>().First();
		genre.Name = "Blah";
		query = adhoc.GetStoredProcedure("metadata.Genre_Save", genre);
		genre = query.Run<Genre>().First();
	}
}
```

This starts with the ad-hoc database connection from the provider and connection
string information registered in the ConnectionLoader. This ad-hoc database
object can be constructed anew every time or cached and reused. Either usage is
fine. It is thread-safe.

Then it creates the Query for the stored procedure and passes a parameter to it.

Next, it runs the Query and maps its default (that is, first) resultset to a
list of Genre objects, then gets the first Genre object from the list and
assigns it to a variable.

Then it updates the Name of the Genre object.

With changes made to the Genre object, a new Query is made to save this data
back to the database. The Genre object, in its entirety, is mapped as a set of
parameters for this new query.

Then the new Query is run and the results are mapped to a list of Genre objects
again, from which the first Genre object is plucked and assigned to a variable.

Notice that the Database, Query, and Genre objects don't need to be disposed.
That's because they don't persist anything from the database connection outside
of the actual Run() and RunAsync() methods.

But what if you don't have a class to map the results to at all? Well, lucky for
you, ADORE has a whole QueryResult object you can use! Remember, ADORE is still
just ADO.NET at heart, so all of the underlying mechanisms of ADO are still
there, out of sight, out of mind. If you just need a data table (or two!), the
QueryResult object has you covered.

The QueryResult object provides:

- A copy of the query that produced this result.
- A copy of the Parameters used to run the query.
- HasError and Exception parameters to facilitate error handling. This has a
  side-effect of preventing database engine errors from being uncaught and
  bubbling up into calling code.
- HasResults provides feedback about whether any resultsets were successfully
  returned.
- A collection of resultsets, with support for named resultsets as well as
  indexed access.
- Object-mapping facilities to map resultsets into collections of
  strongly-typed objects.

These tools provide the flexibility to work with databases on your terms. They
make ADO Really Easy.

## Feature Roadmap

- Text-file database providers (JSON, XML, CSV)
  - ADO.NET read operations
  - Schema detection
  - ADO.NET write operations
- Non-remote query parsing, compiled down to LINQ operations
  - Parsing support for:
    - XPath
    - JSONPath
    - GraphQL
- Code generation
  - From SQL, using INFORMATION_SCHEMA and/or sys
  - From text-file schemas
- Automatic IDistributedCache management
  - Per-entity lifetime configuration
  - Conditional cache overrides
