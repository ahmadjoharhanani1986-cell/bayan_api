using System.Data;
using Dapper;
namespace SHLAPI.Models.Dashboards
{
    public class Dashboards_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int id)
        {
            try
            {
                string where = string.Format(" id={0} ", id);
                string spName = "sp_Dashboards_GetAllByWhere";
                var param = new
                {
                    where
                };
                var res = await db.QueryAsync<dynamic>(
                     spName,
                     param,
                     transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
                return res;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
   
   
    }
}