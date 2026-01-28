using System.Data;
using Dapper;
using SHLAPI.Features.InvoiceVoucher;
using SHLAPI.Models.Settings;


namespace SHLAPI.Models.GetItemByIdOrNo
{
    public class GetItemByIdOrNo_M
    {
        public class ItemSelectedInformation
        {
            public int itemId { get; set; }
            public string itemNo { get; set; }
            public string itemName { get; set; }
            public int unitId { get; set; }
            public string unitName { get; set; }
            public decimal payPrice { get; set; }
            public int mainSellUnitId { get; set; }
            public string mainSellUnitName { get; set; }
            public int mainPayUnitId { get; set; }
            public string mainPayUnitName { get; set; }
            public int typeId { get; set; }
            public decimal toMainUnitQty { get; set; }
            public int wsPriceCurr { get; set; }
            public double wholeSellingPrice { get; set; }
            public int rPriceCurr { get; set; }
            public double interSellingWholeSellPrice { get; set; }
            public double itemTax { get; set; }
            public string barCode { get; set; }
            public string notesInGrid { get; set; }
            public double weight_kg { get; set; }
            public double volume_m3 { get; set; }
            public bool hasExpiryDate { get; set; }
            public int serviceAccountId { get; set; }
            public int suspended { get; set; }
            public double last_pay_price_main_withoutdiscount { get; set; }
            public double last_purchase_price_maint_unit { get; set; }
            public bool Active_Additional_Item { get; set; }
            public bool Add_Additional_Item_Manual_In_Invoice { get; set; }
            public int Item_Option { get; set; }
            public double last_purchase_price { get; set; }
public int lp_price_curr_main_unit { get; set; }
public int lp_price_curr { get; set; }
        }

        public class ItemUnit
        {
            public int unitId { get; set; }
            public string unitName { get; set; }
            public decimal toMainUnitQty { get; set; }
        }

        public class ItemQuantity
        {
            public decimal quantity { get; set; }
        }

        public class ItemAccount
        {
            public int accountId { get; set; }
            public string accountName { get; set; }
        }

        public class ItemStore
        {
            public int storeId { get; set; }
            public int itemId { get; set; }
            public double low_level { get; set; }
            public double high_level { get; set; }
        }

        public class ItemSelectionResult
        {
            public ItemSelectedInformation selectedItemInformation { get; set; }
            public List<ItemUnit> selectedItemUnits { get; set; }
            public ItemQuantity selectedItemQuantity { get; set; }
            public dynamic selectedItemAccount { get; set; }
            public ItemStore selectedItemStore { get; set; }
        }
        public class ItemStockQuantity
        {
            public decimal Quantity { get; set; }
            public decimal ToMainUnitQuantity { get; set; }
        }
        public enum findItemByEnum
        {
            byId = 1,
            byNo = 2,
            byBarcode = 3
        }
        static public string CovertToCodeWithBeginChar(List<Settings_M> settingList, string str)
        {
            string x = "";

            int itemsCodeLength = 0;
            var settingitemsCodeLength = settingList.FirstOrDefault(s => s.id == 9); // ItemsCodeLength
            int.TryParse(settingitemsCodeLength.value + "", out itemsCodeLength);

            var settingItemNoDefaultChar = settingList.FirstOrDefault(s => s.id == 133); // ItemNoDefaultChar

            x = AdjustCode1(str, itemsCodeLength);
            x = AdjustCode(x.ToCharArray());
            if (x.Length > 0)
            {
                char _begin = x.ToCharArray()[0];
                if (_begin != 'I' && _begin != 'S' && _begin != 's' && _begin != 'i' && settingItemNoDefaultChar.value.Trim() != "") // 133
                {
                    x = x.Remove(0, 1);
                    x = settingItemNoDefaultChar.value.Trim() + x;
                }
            }
            return x;
        }
        static string AdjustCode1(string code, int length)
        {
            int count;
            int len = code.Length;
            string newCode = code;

            for (count = len; count < length; count++)
            {
                newCode = newCode + " ";                              // //MLHIDE
            }


            return newCode;
        }
        static string AdjustCode(char[] code)
        {
            int i, c, len;
            len = code.Length;
            string newCode = "";
            for (i = 0, c = -1; i < len && c == -1; i++)
            {
                int s = 0;
                if (int.TryParse(code[i].ToString(), out s))
                {
                    c = i;
                }
            }
            if (c >= 0)
            {
                while (code[len - 1] == ' ')
                    for (i = len - 1; i > c; i--)
                    {
                        if (i == 0)
                            code[i] = ' ';
                        else
                        {
                            code[i] = code[i - 1];
                            code[i - 1] = ' ';
                        }
                    }
            }
            for (i = 0; i < code.Length; i++)
            {
                if (code[i] == ' ')
                    code[i] = '0';
            }
            newCode = new string(code);
            return newCode;
        }
        public static async Task<ItemSelectionResult> GetData(IDbConnection db, IDbTransaction trans,
                                                             GetItemByIdOrNoF.Query obj)
        {
            var result = new ItemSelectionResult();
            List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);

            // if (type == "H")
            //     accountId = BusinessLayer.SHL_Settings.ItemsPurchaseAccount;
            // else if (_billtype == BusinessLayer.Vouchers_And_Bills.BillType.I)
            // {
            //     accountId = BusinessLayer.SHL_Settings.ItemsSellAccount;
            //     getSellUnit = true;
            // }
            // else if (_billtype == BusinessLayer.Vouchers_And_Bills.BillType.S)
            //     accountId = BusinessLayer.SHL_Settings.ItemsSellBackAccount;
            // else if (_billtype == BusinessLayer.Vouchers_And_Bills.BillType.M)
            // {
            //     accountId = BusinessLayer.SHL_Settings.ItemsPurchaseBackAccount;
            //     getSellUnit = true;
            // }

            if (obj.findItemBy == (int)findItemByEnum.byNo)
            {
                obj.itemNo = CovertToCodeWithBeginChar(settingList, obj.itemNo);
            }
            else if (obj.findItemBy == (int)findItemByEnum.byBarcode)
            {
                obj.itemUnit = 0;
                obj.getSellUnit = false;
            }

            int settingIdValue = obj.type == "I" ? 15 : 14;
            int accountIdVal = 0;
            var settingItemsSellAccount = settingList.FirstOrDefault(s => s.id == settingIdValue); // ItemsSellAccount
            int.TryParse(settingItemsSellAccount.value + "", out accountIdVal);
            obj.accountId = accountIdVal;
            obj.getSellUnit = obj.type == "I" ? true:false;
            obj.storeId = 1; // main store
            try
            {
                // 1) SelectedItemInformation
                var sqlInfo = @"
                SELECT TOP(1) i.id AS itemId, i.name AS itemName,i.type_id as typeId,i.no as itemNo, i.service_account_id as serviceAccountId,
                       u.unit_id AS unitId, u.to_main_unit_qty as toMainUnitQty, u.item_unit_pay_price AS payPrice, i.ws_price_curr as wsPriceCurr,
                       i.whole_selling_price as wholeSellingPrice, i.r_price_curr as rPriceCurr,i.inter_selling_wholeSell_price as interSellingWholeSellPrice,
                       u2.name AS unitName, tax.tax_val as itemTax, u.item_unit_bar_code as barCode, weight_kg,volume_m3, i.has_expiry_date as hasExpiryDate,
                       (SELECT TOP(1) unit_id FROM Items_units WHERE item_id = i.id AND main_pay_unit = 'true') AS mainPayUnitId,
                       (SELECT TOP(1) unit_id FROM Items_units WHERE item_id = i.id AND main_sell_unit = 'true') AS mainSellUnitId,
                       (SELECT TOP(1) name FROM Units WHERE id=(SELECT TOP(1) unit_id FROM Items_units WHERE item_id=i.id AND main_sell_unit='true')) AS mainSellUnitName,
                       (SELECT TOP(1) name FROM Units WHERE id=(SELECT TOP(1) unit_id FROM Items_units WHERE item_id=i.id AND main_pay_unit='true')) AS mainPayUnitName,
                       i.Suspended as suspended,i.last_pay_price_main_withoutdiscount,i.last_purchase_price_maint_unit,
                       i.Active_Additional_Item,i.Add_Additional_Item_Manual_In_Invoice,i.Item_Option,
                       i.lp_price_curr,i.lp_price_curr_main_unit,i.last_purchase_price
                FROM Items_and_services i
                LEFT JOIN Items_units u ON i.id = u.item_id
                LEFT JOIN Units u2 ON u2.id = u.unit_id
                Left Join Tax on tax.id = i.item_tax
                WHERE 1=1";
                if (obj.itemUnit != 0) sqlInfo += " and u.unit_id=@itemUnit";
                else if (obj.getSellUnit) sqlInfo += @"AND (
                                                                i.type_id <> 1 
                                                                OR u.main_sell_unit = 'true'
                                                            ) ";
                if (obj.findItemBy == (int)findItemByEnum.byId)
                {
                    sqlInfo += " and i.id = @itemId";
                }
                else if (obj.findItemBy == (int)findItemByEnum.byNo)
                {
                    sqlInfo += " and i.no=@itemNo";
                }
                else if (obj.findItemBy == (int)findItemByEnum.byBarcode)
                {
                    sqlInfo += " and u.item_unit_bar_code=@itemBarCode";
                }


                sqlInfo += @" ORDER BY i.id, u.main_unit DESC";

                var selectedItem = await db.QueryFirstOrDefaultAsync<ItemSelectedInformation>(
                    sqlInfo,
                    new
                    {
                        itemId = obj.itemId,
                        itemUnit = obj.itemUnit,
                        itemBarCode = obj.itemBarCode ?? "",
                        itemNo = obj.itemNo ?? ""
                    },
                    trans
                ) ?? new ItemSelectedInformation();



                bool settingCopyItemBarCodeInNotesVal = false;
                var settingCopyItemBarCodeInNotes = settingList.FirstOrDefault(s => s.id == 143); // CopyItemBarCodeInNotes
                bool.TryParse(settingCopyItemBarCodeInNotes.value, out settingCopyItemBarCodeInNotesVal);
                selectedItem.notesInGrid = "";
                if (settingCopyItemBarCodeInNotesVal)
                {
                    if (selectedItem.barCode != null && selectedItem.barCode.Trim() != "")
                    {
                        selectedItem.notesInGrid = selectedItem.barCode.Trim();
                    }
                }
                bool settingAddItemLocationToItemNoteInSellingBillsVal = false;
                var settingAddItemLocationToItemNoteInSellingBills = settingList.FirstOrDefault(s => s.id == 192); // AddItemLocationToItemNoteInSellingBills
                bool.TryParse(settingAddItemLocationToItemNoteInSellingBills.value, out settingAddItemLocationToItemNoteInSellingBillsVal);
                if (settingAddItemLocationToItemNoteInSellingBillsVal)
                {

                    var parameters = new DynamicParameters();
                    parameters.Add("@Str1", $" where item_id={selectedItem.itemId} and store_id={obj.storeId}");

                    // تنفيذ الـ Stored Procedure
                    var itemLocationList = await db.QueryAsync(
                        "ItemStoresLocation_GetAllByWhere_sp",
                        parameters,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                    if (itemLocationList != null && itemLocationList.AsList().Count > 0)
                    {
                        string itemLocation = itemLocationList.AsList()[0].location_name + "";
                        if (itemLocation.Trim() == "" && obj.storeId == 1) // مستودع رئيسي
                        {
                            itemLocation = itemLocationList.AsList()[0].mainLocationName + "";
                        }
                        if (selectedItem.notesInGrid == "") selectedItem.notesInGrid = itemLocation;
                        else selectedItem.notesInGrid += "/" + itemLocation;
                    }
                }

                result.selectedItemInformation = selectedItem;
                // 2) Units
                var sqlUnits = "SELECT unit_id AS unitId, to_main_unit_qty, '' AS unitName FROM Items_units WHERE item_id = @itemId";
                result.selectedItemUnits = (await db.QueryAsync<ItemUnit>(sqlUnits, new { obj.itemId }, trans)).ToList();

                // 3) Quantity 
                ItemStockQuantity itemQntyResult = null;
                var setting = settingList.FirstOrDefault(s => s.id == 25); // AllowItemNegativeQty
                bool allowItemNegativeQty = false;
                if (setting != null)
                    bool.TryParse(setting.value, out allowItemNegativeQty);
                if (!allowItemNegativeQty && obj.itemId > 0)
                {
                    itemQntyResult = await CalcItemStockQty(db, trans, obj.itemId, obj.itemUnit, obj.storeId);
                    result.selectedItemQuantity = new ItemQuantity { quantity = itemQntyResult.Quantity / itemQntyResult.ToMainUnitQuantity };
                }

                // 4) Account

                //obj.accountId
                if (selectedItem.typeId == 2) // service
                {
                    obj.accountId = selectedItem.serviceAccountId;
                }
                var _obj = await SHLAPI.Models.Currency.Currency_M.CopyCurrenciesExchangePrice(db, trans, obj.voucherDate.Date);
                var query = @"
    SELECT c.*, cp.exchange_price
    FROM ChartOfAccount c
    INNER JOIN Currency_prices cp ON c.currency_id = cp.currency_id
    WHERE cp.date = @Date
      AND c.id = @AccountId";

                result.selectedItemAccount = (await db.QueryAsync<dynamic>(
                    query,
                    new { Date = obj.voucherDate.Date, AccountId = obj.accountId },
                    transaction: trans
                )).FirstOrDefault();

                // 5) Store
                var sqlStore = "SELECT item_id AS itemId, store_id AS storeId, low_level,high_level FROM Items_Stores WHERE item_id=@itemId AND store_id=@storeId";
                result.selectedItemStore = await db.QueryFirstOrDefaultAsync<ItemStore>(sqlStore, new { obj.itemId, obj.storeId }, trans);

            }
            catch (Exception EX)
            {

                throw;
            }
            return result;
        }
        public static async Task<ItemStockQuantity> CalcItemStockQty(IDbConnection db, IDbTransaction trans, int itemId, int unitId, int storeId)
        {
            var result = new ItemStockQuantity { Quantity = 0, ToMainUnitQuantity = 1 };
            try
            {
                // 1) Get to_main_unit_qty
                string sqlUnit = @"SELECT TOP 1 to_main_unit_qty 
                               FROM Items_units 
                               WHERE item_id = @itemId AND unit_id = @unitId";
                var toMainUnitQty = await db.ExecuteScalarAsync<decimal?>(sqlUnit, new { itemId, unitId }, trans);
                if (toMainUnitQty.HasValue)
                    result.ToMainUnitQuantity = toMainUnitQty.Value;

                // 2) Call stored procedure to get stock qty
                var parameters = new DynamicParameters();
                parameters.Add("@str1", itemId);
                parameters.Add("@str2", "null");
                parameters.Add("@str3", "null");
                parameters.Add("@str4", storeId);

                var stockData = await db.QueryFirstOrDefaultAsync(
                    "Items_and_services_GetItemsQuantity_sp",
                    parameters, trans,
                    commandType: CommandType.StoredProcedure);

                if (stockData != null && stockData.Quantity != null)
                    result.Quantity = (decimal)stockData.Quantity;
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;

        }

        public static async Task<IEnumerable<dynamic>> GetItemAdditional(IDbConnection db, int itemId, IDbTransaction? trans = null)
        {
            try
            {
                string where = string.Format(" Items_Additionals.item_id={0}", itemId);
                var result = await db.QueryAsync<dynamic>(
                    "ItemMainAdditional_ByWhere_sp", // اسم الـ SP
                    new
                    {
                      Str1 =  where
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: trans
                );

                return result.ToList();
            }
            catch (Exception EX)
            {
                throw;
            }
        }

    }
}

