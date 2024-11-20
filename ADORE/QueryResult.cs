using ADORE.Extensions;

using System.Data;

namespace ADORE
{
	public class QueryResult
	{
		public static QueryResult Empty = new QueryResult();

		/// <summary>
		/// <para>Exception thrown by the Query.</para>
		/// <para>This will be null if no exception was thrown.</para>
		/// </summary>
		public Exception Exception { get; internal set; } = null;
		/// <summary>
		/// <para>The query parameters, after the query has run.</para>
		/// </summary>
		public QueryParameterCollection Parameters { get; internal set; } = new QueryParameterCollection();
		/// <summary>
		/// <para>The raw resultsets returned by the query.</para>
		/// </summary>
		public DataSet RawResultset { get; internal set; }

		/// <summary>
		/// <para>Indicates whether there was an error returned by the query.</para>
		/// </summary>
		public bool HasError { get => Exception is not null; }
		/// <summary>
		/// <para>Indicates whether any results were returned by the query.</para>
		/// </summary>
		public bool HasResults { get => RawResultset is not null; }
		/// <summary>
		/// <para>The number of resultsets returned by the query.</para>
		/// </summary>
		public int ResultsCount { get => RawResultset?.Tables.Count ?? 0; }

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
