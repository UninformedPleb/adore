# ADORE

ADORE is a backronym for ADO is Really Easy.

Originally, I wanted to name this project EZADO, but that's already taken. So
"ADORE" it is.

The fact is, low-level ADO is tedious in its out-of-the-box form. You have to
manage every connection, every command, every data adapter, and you have to
make sure you clean up after it all. Nothing is automatic, and everything has
some cleanup task or another that needs to be tended to.

The entire reason for ADORE is to simplify ADO code down to its logical
operations. Make a connection, get a query on that connection, run that query,
then get the result. No more futzing around with making sure the connection
state is valid or worrying about whether you remembered to tie up all of the
loose ends afterward.

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