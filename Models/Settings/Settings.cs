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
               string sql = @"SELECT id, description, type, value 
               FROM SL_setting 
               WHERE 1=1 
               ORDER BY id ASC";

return await db.QueryAsync<Settings_M>(sql, transaction: trans);

               
            }
            catch (Exception EX)
            {
                throw;
            }
        }

    }
}