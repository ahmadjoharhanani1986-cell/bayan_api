using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

public class CalcItemQnty_M
{
    public double quantity { get; set; }
    public decimal toMainUnitQuantity { get; set; }

    public static async Task<CalcItemQnty_M> GetData(
        IDbConnection db,
        IDbTransaction trans,
        int itemId,
        int unitId,
        int storeId)
    {
        try
        {
            decimal toMainUnitQuantity = 1;
            double quantity = 0;

            if (itemId == 0)
                return new CalcItemQnty_M { quantity = 0, toMainUnitQuantity = toMainUnitQuantity };

            // Get item unit conversion
            var itemUnitQuery = @"SELECT to_main_unit_qty 
                                  FROM Items_units 
                                  WHERE item_id = @ItemId AND unit_id = @UnitId";

            var itemUnit = await db.QueryFirstOrDefaultAsync<decimal?>(
                itemUnitQuery,
                new { ItemId = itemId, UnitId = unitId },
                transaction: trans
            );

            if (itemUnit.HasValue)
                toMainUnitQuantity = itemUnit.Value;

            // Call the stored procedure Items_and_services_GetItemsQuantity_sp
            var stockQty = await db.QueryFirstOrDefaultAsync<double?>(
                "Items_and_services_GetItemsQuantity_sp",
                new
                {
                    str1 = itemId.ToString(),
                    str2 = "null",
                    str3 = "null",
                    str4 = storeId.ToString()
                },
                transaction: trans,
                commandType: CommandType.StoredProcedure
            );

            if (stockQty.HasValue)
                quantity = stockQty.Value;

            return new CalcItemQnty_M
            {
                quantity = quantity,
                toMainUnitQuantity = toMainUnitQuantity
            };
        }
        catch (Exception EX)
        {
            return new CalcItemQnty_M
            {
                quantity = 0,
                toMainUnitQuantity = 1
            };
        }
    }

public class StockQtyBalanceItem
{
    public string ItemServiceNo { get; set; }
    public string ItemServiceName { get; set; }
    public DateTime? QtyExpireDate { get; set; }
    public decimal Result { get; set; }
}
    public static async Task<IEnumerable<StockQtyBalanceItem>> GetDataByExpiryDate(
         IDbConnection db,
        IDbTransaction trans,
        int itemId,
        int unitId,
        DateTime qtyExpiryDate,
        int storeId)
    {
        List<StockQtyBalanceItem> result = new List<StockQtyBalanceItem>();

        // Get item unit conversion
        decimal toMainUnitQty = 1;
        if (unitId != 0)
        {
            var itemUnit = await db.QueryFirstOrDefaultAsync<decimal?>(
                "SELECT to_main_unit_qty FROM Items_units WHERE item_id=@ItemId AND unit_id=@UnitId",
                new { ItemId = itemId, UnitId = unitId },trans);

            if (!itemUnit.HasValue)
                return null;

            toMainUnitQty = itemUnit.Value;
        }

        string filter;
        if (qtyExpiryDate > new DateTime(2001,1,1))
        {
        filter = $"Items_and_services.id = {itemId} AND vouchers_items_and_services.qty_expire_date = '{qtyExpiryDate:yyyy-MM-dd} 00:00'";
            if (storeId != -1)
                filter += $" AND Vouchers_And_Bills.store_id = {storeId} AND vouchers_items_and_services.qty_expire_date IS NOT NULL";

            filter += " ORDER BY vouchers_items_and_services.qty_expire_date ASC";
        }
        else
        {
            filter = $"Items_and_services.id = {itemId}";
            if (storeId != -1)
                filter += $" AND Vouchers_And_Bills.store_id = {storeId} AND vouchers_items_and_services.qty_expire_date IS NOT NULL";

            filter += " ORDER BY vouchers_items_and_services.qty_expire_date ASC";
        }

        try
        {
            var queryResult = await db.QueryAsync(
                "Items_and_services_GetAllWithExpiryDate_ByWhere_sp",
                new { str1 = filter, str2 = "null", str3 = storeId.ToString() }, trans,
                commandType: CommandType.StoredProcedure);

            if (queryResult == null)
                return null;


        foreach (var row in queryResult)
        {
            decimal qty = 0;
            if (row.Quantity != null && !string.IsNullOrEmpty(row.Quantity.ToString()))
            {
                qty = Math.Round(Convert.ToDecimal(row.Quantity), 5);
                if (unitId != 0)
                    qty /= toMainUnitQty;
            }

            result.Add(new StockQtyBalanceItem
            {
                ItemServiceNo = row.no?.ToString(),
                ItemServiceName = row.name?.ToString(),
                QtyExpireDate = row.qty_expire_date != null ? (DateTime?)row.qty_expire_date : null,
                Result = qty
            });
        }
        }
        catch (Exception EX)
        {
            throw ;
        }
    return result;
    }




}
