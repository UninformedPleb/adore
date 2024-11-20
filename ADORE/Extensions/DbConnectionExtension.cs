using System.Data;
using System.Data.Common;

namespace ADORE.Extensions
{
    public static class DbConnectionExtension
    {
        internal static void ReadyConnection(this DbConnection conn)
        {
            if (conn.State == ConnectionState.Broken) { conn.Close(); }
            if (conn.State == ConnectionState.Closed) { conn.Open(); }
        }
        internal static void ReleaseConnection(this DbConnection conn)
        {
            if (conn.State == ConnectionState.Open || conn.State == ConnectionState.Broken)
            {
                conn.Close();
            }
        }
    }
}
