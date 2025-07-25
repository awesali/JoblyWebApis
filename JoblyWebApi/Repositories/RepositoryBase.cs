using System.Data;
using System.Data.SqlClient;

namespace JoblyWebApi.Repositories.Common
{
    public abstract class RepositoryBase
    {
        protected readonly SqlConnection _conn;
        protected readonly Func<SqlTransaction?> _tx;

        protected RepositoryBase(SqlConnection conn, Func<SqlTransaction?> tx)
        {
            _conn = conn;
            _tx = tx;
        }

        protected void EnsureOpen()
        {
            if (_conn.State != ConnectionState.Open)
                _conn.Open();
        }
    }
}
