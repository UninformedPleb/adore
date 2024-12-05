# ADORE

ADORE is a backronym for ADO is Really Easy.

Originally, I wanted to name this project EZADO, but that's already taken. So
"ADORE" it is.

## Why use ADORE?

Many developers find low-level ADO to be tedious in its out-of-the-box form. 
You have to manage every connection, every command, every data adapter, and you
have to make sure you clean up after it all. Nothing is automatic, and 
everything has some cleanup task or another that needs to be tended to.

ADORE simplifies ADO code down to its logical operations. Make a connection, 
get a query on that connection, run that query, then get the result. No more 
messing around with making sure the connection state is valid or worrying about
whether you remembered to tie up all of the loose ends afterward.

## A Little History

I started ADORE many years ago, developed its concepts in various commercial
projects through a couple of decades, and finally gave it a name and used it
kind of as a tutorial for my personal adoption of Github and Nuget as I broke
away from older, more "corporate" dev tools. I got it to a semi-tested state,
then I let it languish for years. I figured it had been supplanted by Dapper.

But as I began to use Dapper in my professional duties, I found that it was
lacking. It only answers half of the need. Sure, it streamlines the query, but
it completely ignores the connection. You're on your own for that. And ever
since the transition from .NET Framework to .NET Core to just-.NET, nobody has
paid any attention to how to safely, securely, and *simply* wrap the whole
ADO.NET mechanism.

## How to use ADORE

### Getting Started

#### Configure Providers

The first step in any project that uses ADORE, or really ADO.NET, is to
establish the database providers, the code that will handle each different DBMS
in its own special way. Or, if there isn't a specific provider, the ODBC driver
can be used.

For modern .NET, you're responsible for the registration of database providers.
These "providers" are the database access assemblies, like
Microsoft.Data.SqlClient or Npgsql. This part of ADORE is configuration-driven.
Simply add the providers to your all-environments config.

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
full, it will make your database code seamlessly idiomatic with both SQL and
.NET.

#### Integrating ADORE Into Your Codebase

To use ADORE as it is intended, you'll need to implement your database structure
as implementations of the Database, Schema, and Query classes.

##### Examples In This Section

Examples in this section will be built around the following database structure:

Database:
- MusicLibrary (MSSQLServer)

Schemas:
- dbo (the default schema for SQL Server)
- metadata

Tables:
- dbo.Song (SongID, AlbumID, GenreID, Name)
- dbo.Artist (ArtistID, Name)
- dbo.Performance (SongID, ArtistID, PerformanceDate, Instrument)
- dbo.Album (AlbumID, Name, Publisher, CoverArtwork)
- metadata.Genre (GenreID, Name)

Stored Procedures:
- dbo.Song_Load
- dbo.Song_Save
- dbo.Song_GetPerformances
- dbo.Artist_Load
- dbo.Artist_Save
- dbo.Artist_GetPerformances
- dbo.Performance_Load
- dbo.Perforamnce_Save
- dbo.Album_Load
- dbo.Album_Save
- dbo.Album_GetSongs
- metadata.Genre_Load
- metadata.Genre_Save

##### Implementing a Table

Tables are POCOs. There's nothing special about them, really. They serve as a
pre-configured mapping of a single database record.

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
and other things) under a name and allow for security settings to be applied to
all of them as a group. But since these security settings aren't part of our
application's code, it's best just to think of schemas as roughly equivalent to
a namespace.

To represent a database schema, ADORE provides the Schema base class for you to
extend like this:

```C#
public class MetadataSchema : Schema
{
```

First, we need a constructor. These don't really do much beyond attaching this
instance of the class to its parent Database.

```C#
	public MetadataSchema(Database parent) : base(parent) { }
```

Next, we have to implement the Name property getter that the base class expects.
It uses this to make sure the procedure name specifies the schema. This does NOT
prevent you from manually adding a different schema's procedure to this schema
class.

```C#
	public override string Name { get => "metadata"; }
```

Now, you can make methods to create idiomatic stored procedures. This may seem
like a waste of time, but later, the benefits will be clear.

```C#
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
```

Do that for all of the stored procs in the schema, and the schema class is
complete.

```C#
}
```

##### Implementing a Database

Now that we have tables and schemas, we can put them all into an object that
represents the database.

```C#
public class MusicLibraryDatabase : Database
{
```

First, the database needs its schema instance properties.

NOTE: If you follow the usual naming styles for SQL Server, these will be
lower-case public properties. Automated code analysis will probably complain
about this. To sidestep the robotic style police, apply a
SuppressMessageAttribute to these if you wish.

```C#
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public DboSchema dbo { get; private set; }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public MetadataSchema metadata { get; private set; }
```

Next, the constructor. It anchors our database object to its provider factory
and connection string. It also initializes the schema instances.

```C#
	public MusicLibraryDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString)
	{
		this.dbo = new DboSchema(this);
		this.metadata = new MetadataSchema(this);
	}
```

With that, the database class contains a ready-to-use instance of each of its
schemas and is complete.

```
}
```

##### Putting It All Together

To use it via DI, simply put a database object into the constructor of your
DI-participating classes to initialize it. To retrieve data from it, the
database instance from DI provides access to the schema, which then provides
access to the stored procedures and other queries. Calling a stored procedure is
as simple as database.schema.storedProc(parameter1, parameter2).

The example below shows how the Genre object can be retrieved from LoadGenre(),
then updated in-place, then saved back to the database with SaveGenre().

NOTE: This example expects the Genre_Save stored procedure to be written as an
upsert that returns its upserted record, meaning that the return value from
SaveGenre() is the current state of the record that just got loaded, modified,
and saved.

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

Setting things up this way, the semantics match the database structure.

This also lends itself to being templated or generated.

#### Unintegrated Use

All of that structure is great, but what if you just need to run a quick query?
That, too, is available via the AdHocDatabase. AdHocDatabase is a sealed class
that exposes the base-level Database methods CreateQuery and
CreateStoredProcedure. If you don't need the structure outlined above, or if you
just don't want it, here's how to use ADORE with all of the same object-mapping
facilities, minus the overhead of enforcing a readable and concise code
structure.

We can do the same thing as before, but without DI, without database, schema,
and table classes, and without any pre-defined structure. All you need is a
provider factory, a connection string, and work to do!

But this example still uses a table class:

```C#
public class FooThingy
{
	public void DoStuff()
	{
		var adhoc = new AdHocDatabase(ConnectionLoader.GetFactory("MSSQLServer"), ConnectionLoader.GetDatabaseConnection("MusicLibrary"));
		var query = adhoc.GetStoredProcedure("metadata.Genre_Load", new { GenreID = 27 });
		var genre = query.Run<Genre>().First();
		genre.Name = "Blah";
		query = adhoc.GetStoredProcedure("metadata.Genre_Save", genre);
		genre = query.Run<Genre>().First();
	}
}
```

It's a little bit more hassle, and slightly less easy-to-read, when compared to
the fully fleshed-out idiomatic style. But as ADO.NET programming goes, this is
easy. Because ADORE makes **ADO** **R**eally **E**asy.
