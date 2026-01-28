using System.Data;
using Dapper;
namespace SHLAPI.Models.ItemUnits
{
    public class ItemUnits_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int itemId)
        {
            try
            {
                var parameters = new { where = $" item_id={itemId}" };
                var result = await db.QueryAsync<dynamic>(
                    "GetUnitNameFromItemUnits_sp",
                    parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception EX)
            {
                throw;
            }
        }


    }
}