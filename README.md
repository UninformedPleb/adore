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
futzing around with making sure the connection state is valid or worrying about
whether you remembered to tie up all of the loose ends afterward.

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

### Register Providers

The first step in any project that uses ADORE, or really ADO.NET, is to
establish the database providers, the code that will handle each different DBMS
in its own special way. Or, if there isn't a specific provider, the ODBC driver
can be used.

If you're still in the .NET Framework, you don't need to worry about this
because Microsoft preconfigured it all for you in the machine config file.

But for .NET Core and .NET 5+, you will need to handle registration of database
providers manually. These "providers" are the database access assemblies, like
Microsoft.Data.SqlClient or Npgsql. This part of ADORE is configuration-driven.
Simply add the providers to your config:

```JSON
"ADORE": {
	"ProviderFactories": [
		{
			"ProviderName": "MSSQLServer",
			"FactoryTypeName": "Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient"
		},
		{
			"ProviderName": "Postgres",
			"FactoryTypeName": "Npgsql.NpgsqlFactory"
		}
	]
}
```

If you're using DI-style startup, as most projects do these days, ADORE will
automatically register the providers in ADORE:DbProviderFactories when you add
it to your services collection.

If you're a different startup style that doesn't involve DI, like in a legacy
project, you will need to call the DbConnectionLoader.Init() yourself, and pass
a List\<DbProviderFactories> to it.

### Register Connection Strings

The next step is to configure connection strings to each DBMS host and database
(catalog) that you need connections for.

If you're using 














To make a connection, it's as simple as making a connection string. .Net-stack
devs are probably familiar with using the XML config files for this. You can
still do that, or you can make a connection string inline in your code. The
.Net framework even provides the DbConnectionStringBuilder to assist you with
this.

Once you have a connection string, you establish a connection manager for that
connection string. That manager will handle all of the opening, closing, setup,
cleanup, and teardown for every query against that database connection. But
don't worry, it's not strict. You can have multiple objects managing the same
database connection. They'll each connect and disconnect independently.

With that connection manager, you can start transactions, generate query
objects, and adjust timeouts. The most common thing is to generate queries,
since that's how you get things done!

To make a query useful, it needs a query string and, usually, some parameters.
All queries are parameterized, so there's no worry about SQL injection when
using this library as intended. (That's not to say you couldn't do it wrong if
you tried...)

From there, all you have to do is tell the query to run itself! It can run
with or without a result, and that result can be handed back as a scalar value,
a table, or a whole set of tables.

With all of this, the idea is to remove most of the tedium from boilerplate
database code. There will always be a bit of boilerplate, but it should be
simple and easy-to-understand. It should get right to the logic of what you
want to accomplish, without getting bogged down in the technical details of its
underlying implementation. ADORE's purpose is to handle those technical details
for you, but leave the logic to you.