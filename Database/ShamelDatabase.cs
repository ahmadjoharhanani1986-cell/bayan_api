using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


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
                var con = new SqlConnection(connectionString);
                con.Open();
                return con;
            }
            catch(Exception ex) {
                throw ex;
            }
        }
        public IDbTransaction BeginTransaction(IDbConnection con)
        {
            return con.BeginTransaction();
        }
    }
}