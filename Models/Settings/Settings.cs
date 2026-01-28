using System.Data;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Utilities;
using System.Text;
using SHLAPI.Features.Accounts;

namespace SHLAPI.Models.Settings
{
    public class Settings_M
    {
        public int id { get; set; }
        public string description { get; set; }
        public string value { get; set; }
        public static async Task<IEnumerable<Settings_M>> GetData(IDbConnection db, IDbTransaction trans)
        {
            try
            {
                string where = " 1=1 order by id asc";   
                string spName = "sp_SL_setting_GetAllByWhere";
                var param = new
                {
                  where
                };
                var res = await db.QueryAsync<Settings_M>(
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