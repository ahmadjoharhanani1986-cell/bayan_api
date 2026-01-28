using System.Data;
using Dapper;
using SHLAPI.Features.Reports;
namespace SHLAPI.Models.ItemListRpt
{
    public class ItemListRpt_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, GetItemListRptF.Query obj)
        {
            try
            {

                string where = CreatFilterString(obj);
                if (string.IsNullOrEmpty(where))
                    where = " 1=1 ";
                string isDefault = "1";
                if (obj.chkItemDetails) isDefault = "2";
                string isRtl = "R";
                string whereWhenHaveTransactions = "";
                if (obj.chkItemTrans)
                    whereWhenHaveTransactions = " inner join vouchers_items_and_services on vouchers_items_and_services.item_service_id = Items_and_services.id ";
                string addItemStoresLeftJoin = "0";
                if (obj.storeId != -1) addItemStoresLeftJoin = "1";

                string spName = "Items_and_services_List_GetAllByWhere_sp";

                // Params exactly like your old GetData call
                var param = new
                {
                    str1 = where,
                    str2 = "1",
                    str3 = isDefault,
                    str4 = isRtl,
                    str5 = obj.storeId.ToString(),
                    str6 = obj.user_id + "",
                    str7 = whereWhenHaveTransactions,
                    str8 = addItemStoresLeftJoin
                };

                // Execute SP
                var result = await db.QueryAsync<dynamic>(
                    spName,
                    param,
                    transaction: trans,
                    commandTimeout: 180,   // timeout in seconds
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        // الكود قديم كما بالشامل لايت ويندز
        private static string CreatFilterString(GetItemListRptF.Query obj)
        {
            string where = "  1=1 ";                                  // //MLHIDE
            try
            {
                if (obj.fromCode != null && obj.fromCode.Trim() != "")
                {
                    where += " and Items_and_services.no  >='" + obj.fromCode.Trim() + "' "; // //MLHIDE
                }
                if (obj.toCode != null && obj.toCode.Trim() != "")
                {
                    where += "  and Items_and_services.no <='" + obj.toCode.Trim() + "' "; // //MLHIDE
                }
                if (obj.itemName != null && obj.itemName.Trim() != "")
                {
                    string str = Utilities.StringUtil.Search_Specific(obj.itemName.Trim(), "Items_and_services.name"); // //MLHIDE
                    where += " AND (" + str + ")";                    // //MLHIDE
                }
                if (obj.supplier != null && obj.supplier.Trim() != "")
                {
                    string str = Utilities.StringUtil.Search_Specific(obj.supplier.Trim(), "ChartOfAccount.name"); // //MLHIDE
                    where += " AND (" + str + ")";                    // //MLHIDE
                }
                if (obj.chkExpiry)
                {
                    where += " and Items_and_services.has_expiry_date=" + 1; // //MLHIDE
                }

                if (obj.chkLess)
                {
                    if (obj.chkEqual)
                    {
                        where += " and case when QuantityCTE.result is  null then 0 else QuantityCTE.result end <= case when (QuantityCTE.low is null or QuantityCTE.low =-1) then 0 else QuantityCTE.low end "; // //MLHIDE
                    }
                    else
                    {
                        where += " and case when QuantityCTE.result is  null then 0 else QuantityCTE.result end < case when (QuantityCTE.low is null or QuantityCTE.low =-1) then 0 else QuantityCTE.low end "; // //MLHIDE
                    }
                }
                if (obj.chkAbove)
                {
                    if (obj.chkEqual)
                    {
                        where += " and case when QuantityCTE.result is  null then 0 else QuantityCTE.result end >= case when (QuantityCTE.low is null or QuantityCTE.low =-1) then 0 else QuantityCTE.low end "; // //MLHIDE
                    }
                    else
                    {
                        where += " and case when QuantityCTE.result is  null then 0 else QuantityCTE.result end > case when (QuantityCTE.low is null or QuantityCTE.low =-1) then 0 else QuantityCTE.low end "; // //MLHIDE
                    }
                }
                else if (obj.chkEqual)
                {
                    where += " and case when QuantityCTE.result is  null then 0 else QuantityCTE.result end = case when (QuantityCTE.low is null or QuantityCTE.low =-1)  then 0 else QuantityCTE.low end  "; // //MLHIDE
                }

                if (obj.chkSuspendedItem)
                    where += " and Items_and_services.Suspended='true' "; // //MLHIDE
                else
                    where += " and Items_and_services.Suspended='false' "; // //MLHIDE
                return where;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}