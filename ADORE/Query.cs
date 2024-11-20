using System.Data;
using System.Data.Common;

using ADORE.Extensions;

namespace ADORE
{
    public class Query
	{
		protected Database _parent;

		/// <summary>
		/// <para>The query text.</para>
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// <para>The SQL statements to run.</para>
		/// <para>This is an alias of Query.Text</para>
		/// </summary>
		public string Sql { get => Text; set => Text = value; }
		/// <summary>
		/// <para>The command type of this Query.</para>
		/// <para>Default is CommandType.Text.</para>
		/// </summary>
		public CommandType CommandType { get; set; } = CommandType.Text;
		/// <summary>
		/// <para>The parameters of this query.</para>
		/// </summary>
		public QueryParameterCollection Parameters { get; set; } = new QueryParameterCollection();

		// ctor internal so only Database can make one
		internal Query(Database parent) { }

		/// <summary>
		/// <para>Runs the query.</para>
		/// </summary>
		/// <returns>A QueryResult object containing details about the run. Resultsets, scalar values, success/failure states, and exceptions are all included in the result object.</returns>
		public QueryResult Run()
		{
			// create the result object. this will get filled in as we go.
			QueryResult result = new QueryResult();

			// sanity-check:
			// if the query text is blank, we can't run it.
			if(string.IsNullOrEmpty(Text)) { return result; }

			// get a connection OUTSIDE the try/finally so we can make sure it always gets closed
			using DbConnection conn = _parent.Factory.CreateConnection();

			try
			{
				// set up the command from the query values
				using var cmd = conn.CreateCommand();
				cmd.CommandText = Text;
				cmd.CommandType = CommandType;
				// map parameters to the command
				Parameters.MapToCommand(cmd);

				// run the command
				conn.ReadyConnection();
				DataSet ds = new DataSet();
				using var da = _parent.Factory.CreateDataAdapter();
				da.SelectCommand = cmd;
				da.Fill(ds);
				conn.ReleaseConnection();
				// map parameters back from the command
				Parameters.MapFromCommand(cmd);

				// collate the results
				result.RawResultset = ds;
				result.Parameters = Parameters;
			}
			catch(Exception ex)
			{
				result.Exception = ex;
			}
			finally
			{
				if(conn is not null) { conn.ReleaseConnection(); }
			}

			return result;
		}
		/// <summary>
		/// <para>Runs the query, returning the first resultset as a collection of T.</para>
		/// </summary>
		/// <typeparam name="T">The mapped return type.</typeparam>
		/// <returns>An IEnumerable collection of T.</returns>
		public IEnumerable<T> Run<T>()
		{
			var result = Run();
			return result.MapResults<T>(0);
		}
		/// <summary>
		/// <para>Runs the query, returning the first scalar result as a T.</para>
		/// </summary>
		/// <typeparam name="T">The scalar return type.</typeparam>
		/// <returns>The scalar value.</returns>
		public T RunScalar<T>()
		{
			var result = Run();
			return result.MapScalarValue<T>(0);
		}
	}
}
