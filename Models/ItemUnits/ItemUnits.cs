using System.Data;
using Dapper;
namespace SHLAPI.Models.ItemUnits
{
    public class ItemUnits_M
    {
    public static async Task<IEnumerable<dynamic>> GetData(
    IDbConnection db,
    IDbTransaction trans,
    int itemId)
{
    try
    {
        string sql = @"
        SELECT DISTINCT 
            iu.item_id,
            iu.unit_id,
            iu.main_unit,
            iu.to_main_unit_qty,
            iu.item_unit_bar_code,
            u.name,
            iu.main_pay_unit,
            iu.main_sell_unit,
            iu.item_unit_pay_price
        FROM Items_units iu
        INNER JOIN Units u ON u.id = iu.unit_id
        WHERE iu.item_id = @itemId;
        ";

        var result = await db.QueryAsync<dynamic>(
            sql,
            new { itemId },
            transaction: trans,
            commandType: CommandType.Text
        );

        return result;
    }
    catch
    {
        throw;
    }
}



    }
}