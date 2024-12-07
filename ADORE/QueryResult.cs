using ADORE.Extensions;

using System.Data;

namespace ADORE
{
	/// <summary>
	/// <para>Represents a complete result from a Query run.</para>
	/// </summary>
	public class QueryResult
	{
		/// <summary>
		/// <para>A blank query result, used for comparison or as a no-op.</para>
		/// </summary>
		public static readonly QueryResult Empty = new QueryResult();

		/// <summary>
		/// <para>The original query string that was run.</para>
		/// </summary>
		public string QueryString { get; init; } = string.Empty;
		/// <summary>
		/// <para>The query parameters, after the query has run.</para>
		/// </summary>
		public QueryParameterCollection Parameters { get; internal set; } = new QueryParameterCollection();
		/// <summary>
		/// <para>The raw resultsets returned by the query.</para>
		/// </summary>
		public DataSet RawResultset { get; internal set; } = null;
		/// <summary>
		/// <para>Exception thrown by the Query.</para>
		/// <para>This will be null if no exception was thrown.</para>
		/// </summary>
		public Exception Exception { get; internal set; } = null;

		/// <summary>
		/// <para>Indicates whether there was an error returned by the query.</para>
		/// </summary>
		public bool HasError => Exception is not null;
		/// <summary>
		/// <para>Indicates whether any results were returned by the query.</para>
		/// </summary>
		public bool HasResults => RawResultset is not null;
		/// <summary>
		/// <para>The number of resultsets returned by the query.</para>
		/// </summary>
		public int ResultsCount => RawResultset?.Tables.Count ?? 0;
		/// <summary>
		/// <para>Gets a resultset from the specified table index</para>
		/// </summary>
		/// <param name="index">The table index</param>
		/// <returns>A datatable containing the resultset</returns>
		public DataTable this[int index] => RawResultset?.Tables[index];
		/// <summary>
		/// <para>Gets a resultset from the specified table by name</para>
		/// </summary>
		/// <param name="tableName">The name of the table</param>
		/// <returns>A datatable containing the resultset</returns>
		public DataTable this[string tableName] => RawResultset?.Tables[tableName];

		// ctor internal so only Query can make one
		internal QueryResult() { }

		/// <summary>
		/// <para>Gets the results, indexed by resultset number, mapped to a type</para>
		/// </summary>
		/// <typeparam name="T">The type to map the resultset records into.</typeparam>
		/// <param name="index">The index of the resultset. Defaults to 0.</param>
		/// <returns>The mapped resultset</returns>
		public IEnumerable<T> MapResults<T>(int index = 0)
		{
			// check that index...
			if(index >= ResultsCount) { throw new ArgumentOutOfRangeException(); }

			// build a list of records mapped to T objects
			List<T> tlist = new List<T>();
			foreach(DataRow dr in RawResultset.Tables[index].Rows)
			{
				tlist.Add(dr.MapTo<T>());
			}
			return tlist;
		}
		/// <summary>
		/// <para>Gets the scalar value of a given resultset by index.</para>
		/// <para>The scalar value is defined as the first column of the first row of the resultset.</para>
		/// </summary>
		/// <typeparam name="T">The type to map the resultset scalar value into.</typeparam>
		/// <param name="index">The index of the resultset. Defaults to 0.</param>
		/// <returns>The mapped scalar value.</returns>
		public T MapScalarValue<T>(int index = 0)
		{
			// check that index...
			if(index >= ResultsCount) { throw new ArgumentOutOfRangeException(); }

			// return the first column, first row from the table at index, cast to T.
			// this may throw an InvalidCastException.
			return (T)RawResultset.Tables[index].Rows[0][0];
		}
	}
}
