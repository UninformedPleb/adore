using System.Data;

namespace ADORE.Extensions
{
    internal static class DbConnectionExtension
    {
		/// <summary>
		/// <para>Ensures the connection is open and ready to use.</para>
		/// <para>There are no side-effects from repeated calls to this method.</para>
		/// </summary>
		/// <param name="conn">The connection to get ready</param>
        internal static void ReadyConnection(this IDbConnection conn)
        {
            if (conn.State == ConnectionState.Broken) { conn.Close(); }
            if (conn.State == ConnectionState.Closed) { conn.Open(); }
        }
		/// <summary>
		/// <para>Ensures the connection is cleaned up and closed.</para>
		/// <para>There are no side-effects from repeated calls to this method.</para>
		/// </summary>
		/// <param name="conn">The connection to release</param>
		internal static void ReleaseConnection(this IDbConnection conn)
        {
            if (conn.State == ConnectionState.Open || conn.State == ConnectionState.Broken)
            {
                conn.Close();
            }
        }
    }
}
