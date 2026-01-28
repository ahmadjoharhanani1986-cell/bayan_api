using System.Data;
using Dapper;
namespace SHLAPI.Models.ItemPrices
{
    public class ItemPrices_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int itemId, int unitId)
        {
            try
            {
                string where = $" Items_and_services.[id]={itemId} " +
                    $" and Items_units.item_id={itemId} " +
                    $" and Items_units.unit_id={unitId}";

                var parameters = new { Str1 = where };

                var result = await db.QueryAsync<dynamic>(
                    "ItemAndService_GetAllByItemId_WithCurrName",   // اسم SP
                    parameters,
                    trans,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        public static async Task<IEnumerable<dynamic>> GetLastPayPriceForItem(IDbConnection db, IDbTransaction trans, int itemId, int lang_id)
        {
            try
            {
                string selectStatment = @" SELECT *
                                                FROM (
                                                    SELECT 
                                                        1 AS _type, case when @lang_id =1 then 'اخر سعر شراء' else 'last purchase price ' end as colName,
                                                        Items_and_services.id,
                                                        Items_and_services.last_purchase_price AS price,
                                                        Currency.name AS currency_name
                                                    FROM Items_and_services
                                                    LEFT JOIN Currency ON Currency.id = Items_and_services.lp_price_curr 
                                                    where Items_and_services.id = @itemId

                                                    UNION ALL

                                                    SELECT 
                                                        2 AS _type, case when @lang_id =1 then 'اخر سعر شراء عملة رئيسية' else 'last purchase price main unit' end as colName,
                                                        Items_and_services.id,
                                                        Items_and_services.last_purchase_price_maint_unit AS price,
                                                        Currency.name AS currency_name
                                                    FROM Items_and_services
                                                    LEFT JOIN Currency ON Currency.id = Items_and_services.lp_price_curr_main_unit
                                                        where Items_and_services.id = @itemId
                                                ) AS combined
                                                ORDER BY id, _type;
                                                ";
                var res = await db.QueryAsync<dynamic>(
                    selectStatment,
                    new
                    {
                        itemId,
                        lang_id
                    },
                    trans
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