using System.Data;

namespace SHLAPI.Database
{
    public interface IMasterDatabase
    {
         IDbConnection Open();
         IDbTransaction BeginTransaction(IDbConnection con);
    }
}