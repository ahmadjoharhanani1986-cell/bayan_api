using System.Data;

namespace SHLAPI.Database
{
    public interface IShamelDatabase
    {
         IDbConnection Open();
         IDbTransaction BeginTransaction(IDbConnection con);
    }
}