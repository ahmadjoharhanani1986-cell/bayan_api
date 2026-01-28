using System;
using System.Data;
using System.Data.SqlClient;

namespace SHLAPI.Database
{
    public class MasterDatabase : IMasterDatabase
    {
        private static string connectionString = null;
        public IDbConnection Open()
        {
            try
            {
                if (connectionString == null)
                {
                    connectionString = Config.GetInstance().MasterConnectionString;
                }

                var con = new SqlConnection(connectionString);
                con.Open();
                return con;
            }
            catch(Exception ex) 
            {
                throw ex;
            }
        }
        public IDbTransaction BeginTransaction(IDbConnection con)
        {
            try
            {
                return con.BeginTransaction();
            }
            catch { }
            return null;
        }
    }
}