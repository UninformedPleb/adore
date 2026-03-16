# ADORE
ADORE is a backronym for **ADO** is **R**eally **E**asy. Yes, it's kinda corny,
but it states the project's overall goal quite nicely.

## Why use ADORE?
Many developers find low-level ADO.NET to be tedious. I can't really blame them.
Every connection, every command, every data adapter has to be managed and
cleaned up by *you*. Nothing is automatic or easy.

ADORE simplifies ADO.NET by using configurations to pre-initialize much of the
boilerplate stuff for you. After it's configured and you call the built-in
initialization, all you have to do is use a Database to make a Query and run it.

Of course, there are also additional features that can help you to keep a large
project organized. You can build a repository-pattern library with ease. It even
lends itself well to code-generation. Check the roadmap for features being
planned for future releases.

### But... Why use ADORE instead of EF or Dapper?
Good question. Those are certainly the most well-known "easy" data access
systems in the .NET ecosystem. But EF requires 100% commitment to a very heavy
abstraction, and Dapper doesn't handle any of the provider context pieces of the
puzzle. ADORE is meant to be a simplification of ADO.NET itself. It falls
somewhere in between ADO.NET's toolbox approach and EF's bubble-wrapped style.

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
  - Conditional cache overrides and bypasses
- More ORM-ish functionality
  - Table mappings with auto-CRUD
  - Column-spec generation
  - Filter type generation

## How to use ADORE
### Configure Providers
The first step is to establish the database providers. ADO.NET uses the provider
pattern so each database engine can be used the same way despite their internal
differences. Old .NET used to configure the most common providers for you, but
now you're responsible for registering these libraries.

ADORE has its own configuration section defined so you can add these to your
all-environments configuration, like this:
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

### Configure Connection Strings
Connection strings are also part of the ADORE configuration section. These most
likely belong in the per-environment configuration files:
```JSON
"ADORE": {
	"ConnectionStrings": [
		{
			"ConnectionName": "customdb",
			"ProviderName": "MSSQLServer",
			"ConnectionStringValues": {
				"Server": "customdb.example.com",
				"Database": "Stuff",
				"User ID": "USERNAME",
				"Encrypt": "Optional",
				"TrustServerCertificate": "True"
			}
		},
		{
			"ConnectionName": "anotherdb",
			"ProviderName": "Postgres",
			"ConnectionStringValues": {
				"Server": "10.10.10.10",
				"Port": "5432",
				"Database": "whatever",
				"Uid": "USERNAME"
			}
		}
	]
}
```

And don't forget that you should *never* store passwords in plaintext. Your
secrets file (for local development) or secure keystore (for higher
environments) should store sensitive keys like this:
```JSON
"ADORE": {
	"ConnectionStrings": [
		{
			"ConnectionStringValues": {
				"Password": "YOUR_PASSWORD_HERE"
			}
		},
		{
			"ConnectionStringValues": {
				"Pwd": "YOUR_PASSWORD_HERE"
			}
		}
	]
}
```

### Startup
Most modern .NET applications use a builder pattern to start up an application.
The default behavior is to use Microsoft's dependency injection (DI) system.
ADORE is built to embrace this style. Using it, you need only one line of code:
```C#
builder.ConfigureAdore(builder.Configuration.GetSection("ADORE").Get<AdoreConfig>());
```
With this, the AdoreConfig and the ADORE ConnectionRegistry are registered into
DI, ADO.NET providers are registered into DbProviderFactories, and connection
strings are loaded into the ConnectionRegistry.

If you choose not to use a builder or DI, or if your project type does not
support one, the ConnectionRegistry can be initialized manually this way:
```C#
AdoreConfig config = ... // manually build this or read it from a file
ConnectionRegistry cr = new() { Configuration = config };
cr.RegisterProviders();
cr.RegisterConnections();
```
This ConnectionRegistry is needed whenever you instantiate a new ADORE database,
so you might want to keep it around for the lifetime of your application.

### Databases, Catalogs, and Schemas
A database, sometimes called a catalog, is represented by an object derived from
ADORE's Database class. You can derive your own databases or you can use the
built-in AdHocDatabase class.

If you derive your own databases, they will contain the code to reference that
database's objects, such as schemas, stored procedures, functions, or even
in-application processing that is specific to that database. These are really
just a convenience and some syntactic sugar. And they're a convenient way to
give code generators a place to put their code.

The main purpose of *every* Database, including the AdHocDatabase, is:
- To keep a reference to the DbProviderFactory for its DBMS.
- To keep a reference to the connection string needed to access the DBMS.
- To produce Query objects that can be run against the DBMS.

Once you've implemented your Databases or decided to use the AdHocDatabase, you
register them into DI like this:
```C#
builder.Services.RegisterDatabase<MyCustomDatabase>("customdb");
builder.Services.RegisterDatabase<AdHocDatabase>("anotherdb");
```
The generic type parameter is the Database-derived type, and the name passed to
the method is the ConnectionName of its connection string.

Without DI, you will need to do a little more legwork. The ConnectionRegistry
has the DbProviderFactory and connection string already, so it looks more like
this:
```C#
var customdb = cr["customdb"];
MyCustomDatabase mydb = new(customdb.factory, customdb.config.ConnectionString);
var anotherdb = cr["anotherdb"];
AdHocDatabase adhoc = new(anotherdb.factory, anotherdb.config.ConnectionString);
```

ADORE also maintains the concept of a Schema. Not all database engines use them.
But the ones that do can use it as a pseudo-namespace for functions and stored
procedures, as well as a security group. Like Databases, Schemas are made to
contain code that sends and retrieves data.

### Queries and Results
Once you have a Database object, you're ready to start making and running
queries. A query is any text-based series of instructions that can be
interpreted as a way to locate a specific set of data in a database. ADORE makes
no distinctions between queries that retrieve data and those that make changes
to a database. All of them use the Query class.

A Query is created by a Database, and is only usable within the Database that
created it. A Schema can also create Query objects for its parent Database. To
make a Query, simply do this:
```C#
var query = mydb.CreateQuery("SELECT * FROM Products WHERE Color = 'red'");
```
This will retrieve a list of products with the value "red" in their Color
column.

But what if you want the user to select the color? Set a parameter. Assuming the
color is stored in the `userColor` variable, you would do this:
```C#
var query = mydb.CreateQuery("SELECT * FROM Products WHERE Color = @Color", new { Color = userColor });
```
Some database providers, such as ODBC, do not support named parameters. For
those, you can do this:
```C#
var query = mydb.CreateQuery("SELECT * FROM Products WHERE Color = ?", userColor);
```

So now you have a query, but how do you actually get results? You have to *run*
the query.
```C#
var results = query.Run();
```

Or if you're in a hurry and don't need to keep the Query object, just chain the
call to .Run():
```C#
var results = mydb.CreateQuery("SELECT * FROM Products WHERE Color = @Color", new { Color = userColor }).Run();
```

Query.Run() handles all of the connection, command, parameters, and other
technical minutiae for you and returns a QueryResult. What's a QueryResult? It's
an object that contains *all* of the results, statuses, errors, and anything
else returned by the DBMS running the query.

The QueryResult contains:
- A copy of the query text that produced this result.
- A copy of the Parameters used to run the query.
- HasError and Exception parameters to facilitate error handling. This has a
  side-effect of preventing database engine errors from being uncaught and
  bubbling up into calling code.
- HasResults provides feedback about whether any resultsets were successfully
  returned.
- A collection of resultsets, with support for access to resultsets by name and
  zero-indexed number.
- Object-mapping facilities to map resultsets into collections of
  strongly-typed objects.

So back to the example, how do you get the Product data from the QueryResult?
Well, since there was only one statement in the query's text, the result of that
SELECT will be the first result in the set. The following gets the first ADO.NET
DataTable in the result.
```C#
var resultTable = results[0];
```
You might want to use that DataTable directly, or you might want to bind it to
something that already plays nice with ADO.NET. But what if you're writing
something a little more modern, and you want a collection of objects? Well,
assuming you made a Product object with the same structure as the Product table:
```C#
var products = results.MapResults<Product>();
```
Of course, it would be nice if we didn't have to get results first, then map
them to a collection. Most of the time, we don't need all of the features of the
QueryResult object. So that's an option too:
```C#
var products = query.Run<Product>();
```

Once again, ADORE makes ADO Really Easy.

## Getting the Most Out of ADORE
You can use ADORE without fully integrating it. There's an AdHocDatabase that
can provide basic one-off access to everything you need to run a quick query.
But ADORE really shines when you use its structured Database and Schema
classes to the full. It can make your database code seamlessly idiomatic with
both SQL and .NET.

### Implementing a Schema (Optional)
A schema groups database objects (tables, views, stored procedures, functions,
and other things) under a name and allows for security settings to be applied
as a group. But since these security settings aren't part of our application's
code, schemas treated mostly as a namespace.

To represent a database schema, ADORE provides the Schema base class for you to
extend like this:
```C#
public class MetadataSchema : Schema
{
	public MetadataSchema(Database parent) : base(parent) { }
	public override string Name { get => "metadata"; }

	public async ProductType? LoadProductType(int id)
	{
		var proc = CreateStoredProcedure("LoadProductType", new { ProductTypeID = id });
		return (await proc.RunAsync<ProductType>()).FirstOrDefault();
	}
	public async ProductType SaveProductType(ProductType pt)
	{
		var proc = CreateStoredProcedure("SaveProductType", pt);
		return (await proc.RunAsync<ProductType>()).First();
	}
}
```
The things it needs are prety simple:
- **The constructor**: It ties this schema instance to its parent Database.
- **The Name property getter**: The Schema base class uses this to inject the schema
name into the query text to disambiguate this schema's stored procedures from
other schemas stored procedures. Some databases can be strict about this, so
ADORE is always strict about it.
- **Methods to represent stored procedures**: This makes the stored procedures feel
idiomatic when called by your application's code.

### Implementing a Database
The database is the glue that holds all of the schemas together. It also serves
as a sort of "root" for everything relating to a given database's structure.
```C#
public class StuffDatabase : Database
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public DboSchema dbo { get; private set; }
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches database naming conventions")]
	public MetadataSchema metadata { get; private set; }

	public StuffDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString)
	{
		this.dbo = new DboSchema(this);
		this.metadata = new MetadataSchema(this);
	}
}
```
This one is even simpler than the Schema:
- **Member schemas**: These are instances of the schemas in this database. They
can be flagged with the SuppressMessageAttribute, as seen above, if you need to
bypass strict naming convention rules. Of course, there's no requirement that
these be named exactly as they are in the database. It's just a suggestion.
- **The constructor:** It anchors our database object to its provider factory
and connection string. It also initializes the schema instances.

This is the class that would be registered with the DI container.

### Putting It All Together
To use the database via DI, put it into the constructor of your DI-participating
classes. Retrieving data is as simple as calling the methods you've added to the
schema.
```C#
public class FooThingy(ILogger _logger, StuffDatabase _stuffdb)
{
	public void DoStuff()
	{
		var ptype = _stuffdb.metadata.LoadProductType(27); // magic number is magic... it's an example, okay?
		ptype.Name = "Blah";
		ptype = _stuffdb.metadata.SaveProductType(ptype);
		_logger.LogInformation($"did stuff. product type: {ptype.ProductTypeID}, {ptype.Name}");
	}
}
```
By setting things up this way, the semantics match the database structure. It's
also easily templated or generated.
