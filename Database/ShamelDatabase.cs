using MySqlConnector;   // مهم
using System.Data;

namespace SHLAPI.Database
{
    public class ShamelDatabase : IShamelDatabase
    {
        private static string connectionString = null;

        public IDbConnection Open()
        {
            try
            {
                if (connectionString == null)
                {
                    connectionString = Config.GetInstance().ConnectionString;
                }

                Serilog.Log.Information("ConnectionString " + connectionString);

                var con = new MySqlConnection(connectionString);
                con.Open();
                return con;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IDbTransaction BeginTransaction(IDbConnection con)
        {
            return con.BeginTransaction();
        }
    }
}
