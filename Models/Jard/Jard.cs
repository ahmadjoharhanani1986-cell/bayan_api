using System.Data;
using Dapper;
using SHLAPI.Models.GetItemByIdOrNo;


namespace SHLAPI.Models.Jard
{
    public class Jard_M
    {
        public int id { get; set; }
        public string description { get; set; }
        public int store_id { get; set; }
        public DateTime date { get; set; }
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans)
        {
            try
            {
                string where = " jard_status=1 order by id asc";
                string spName = "sp_Jard_GetAllByWhere";
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
    public class JardTransaction_M
    {


        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int jardId)
        {
            try
            {

                string _where = string.Format(" where jard_id = {0} and voucher_id is null ", jardId); //MLHIDE
                string _selectStatement = @"
                                            select distinct 
                                                ROW_NUMBER() OVER (ORDER BY Jard_Transactions.id) AS ser, 
                                                Jard_Transactions.*, 
                                                Jard_Transactions.id,
                                                '' as defualtUnit,
                                                Jard_Transactions.item_service_id as Items_id,
                                                Jard_Transactions.unit_id as unit_id,
                                                Items_and_services.no  AS Items_no, 
                                                Jard_Transactions.jard_quanty as Items_quantity,
                                                Items_and_services.name as Items_name,
                                                Units.Name as unitName,
                                                to_main_unit_qty,
                                                Items_and_services.has_expiry_date as haveExpiery
                                            from Jard_Transactions
                                            inner join Items_and_services on Items_and_services.id = Jard_Transactions.item_service_id
                                                CROSS APPLY (
                                                SELECT TOP 1 *
                                                FROM Items_units iu
                                                WHERE iu.item_id = Jard_Transactions.item_service_id
                                                AND iu.unit_id = Jard_Transactions.unit_id
                                                ORDER BY iu.id 
                                            ) iu
											inner join Units on Units.id = Jard_Transactions.unit_id
                                            and iu.unit_id = Jard_Transactions.unit_id
                                        ";
                var parameters = new DynamicParameters();
                parameters.Add("@Str1", _selectStatement);
                parameters.Add("@Str2", _where);

                var res = await db.QueryAsync<dynamic>(
                    "dynamic_search_bySelectStatment_sp",
                    parameters,
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

        public static async Task<IEnumerable<dynamic>> GetItemMainUnit(IDbConnection db, IDbTransaction trans, int itemId)
        {
            try
            {
                string where = string.Format(" item_id={0} and main_unit ='true'", itemId);
                string spName = "sp_Items_units_GetAllByWhere";
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
        public class JardTransObj
        {
            public int id { get; set; }
            public int unit_id { get; set; }
            public int item_service_id { get; set; }
            public DateTime expiery_date { get; set; }
            public float jard_quanty { get; set; }
            public float computer_quanty { get; set; }
            public float to_main_unit_qty { get; set; }
            public int jard_id { get; set; }
            public DateTime jardDate { get; set; }
            public int voucher_id { get; set; }
        }
        public static async Task<bool> SaveOrUpdateAllJardItems(IDbConnection db, IDbTransaction trans,
                                                                int jardId, int storeId, int userId,
                                                                 List<JardTransObj> list)
        {
            try
            {
                var sqlDeleteByJard = "DELETE FROM Jard_Transactions WHERE jard_id = @jardId;";
                await db.ExecuteAsync(sqlDeleteByJard, new { jardId }, transaction: trans);
                foreach (JardTransObj obj in list)
                {
                    DateTime entryDate = DateTime.Now;
                    decimal computer_quanty = 0;
                    DateTime Expiery_Date = obj.expiery_date;// fill it from param
                    int itemUnitId = obj.unit_id;
                    if (Expiery_Date.Date != new DateTime(1999, 1, 1).Date)
                    {
                        var itemMainUnit = await GetItemMainUnit(db, trans, obj.item_service_id); // item main unit 
                        if (itemMainUnit != null && itemMainUnit.AsList().Count > 0)
                        {
                            itemUnitId = itemMainUnit.AsList()[0].unit_id;
                        }
                        List<CalcItemQnty_M.StockQtyBalanceItem> listStockQtyBalanceItem = (List<CalcItemQnty_M.StockQtyBalanceItem>)await CalcItemQnty_M.GetDataByExpiryDate(db, trans, obj.item_service_id, itemUnitId, Expiery_Date, storeId);
                        if (listStockQtyBalanceItem != null && listStockQtyBalanceItem.Count > 0)
                        {
                            CalcItemQnty_M.StockQtyBalanceItem qntyRes = listStockQtyBalanceItem[0];
                            computer_quanty = qntyRes.Result;
                        }
                    }
                    GetItemByIdOrNo_M.ItemStockQuantity itemQntyResult = await GetItemByIdOrNo_M.CalcItemStockQty(db, trans, obj.item_service_id, itemUnitId, storeId);
                    if (itemQntyResult != null)
                    {
                        computer_quanty = itemQntyResult.Quantity;
                    }

                    #region insert into JardTrans Table
                    string sql = @"
                                    INSERT INTO Jard_Transactions 
                                    (
                                        item_service_id,
                                        unit_id,
                                        date,
                                        entry_date,
                                        user_id,
                                        jard_id,
                                        jard_quanty,
                                        computer_quanty,
                                        voucher_id,
                                        expiery_date
                                    )
                                    VALUES
                                    (
                                        @item_service_id,
                                        @unit_id,
                                        @date,
                                        @entry_date,
                                        @user_id,
                                        @jard_id,
                                        @jard_quanty,
                                        @computer_quanty,
                                        @voucher_id,
                                        @expiery_date
                                    );

                                    SELECT CAST(SCOPE_IDENTITY() AS int);
                                    ";

                    var param = new
                    {
                        obj.item_service_id,
                        obj.unit_id,
                        date = obj.jardDate,
                        entry_date = entryDate,
                        user_id = userId,
                        jard_id = jardId,
                        obj.jard_quanty,
                        computer_quanty,

                        // ★ NULL if 0
                        voucher_id = obj.voucher_id == 0 ? (int?)null : obj.voucher_id,

                        // ★ NULL if expiery < 2000/01/01
                        expiery_date = obj.expiery_date < new DateTime(2000, 1, 1)
                                        ? (DateTime?)null
                                        : obj.expiery_date
                    };

                    int newId = await db.ExecuteScalarAsync<int>(
                        sql,
                        param,
                        transaction: trans
                    );

                    #endregion end insert into JardTrans Table

                    // else // update
                    // {
                    //     #region update jardTrans Table
                    //     string spName = "sp_Jard_Transactions_Update";

                    //     var param = new
                    //     {
                    //         obj.id,
                    //         obj.item_service_id,
                    //         obj.unit_id,
                    //         jardDate = obj.jardDate,
                    //         entryDate,
                    //         userId,
                    //         jardId,
                    //         obj.jard_quanty,
                    //         computer_quanty,

                    //         // RULE: if voucher_id = 0 → NULL
                    //         voucher_id = obj.voucher_id == 0 ? (int?)null : obj.voucher_id,

                    //         // RULE: if expiery_date < 2000/01/01 → NULL
                    //         expiery_date = obj.expiery_date < new DateTime(2000, 01, 01)
                    //             ? (DateTime?)null
                    //             : obj.expiery_date
                    //     };

                    //     await db.ExecuteAsync(
                    //         spName,
                    //         param,
                    //         transaction: trans,
                    //         commandType: CommandType.StoredProcedure
                    //     );
                    //     #endregion end update jardTrans Table
                    // }
                }
                return true;

            }
            catch (Exception EX)
            {
                throw;
            }
        }
    }
}