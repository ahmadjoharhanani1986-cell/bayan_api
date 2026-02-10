using System.Collections;
using System.Data;
using Dapper;
using SHLAPI.Features.InvoiceVoucher;
using SHLAPI.Models.Settings;

namespace SHLAPI.Models.InvoiceVoucher
{
    public class InvoiceVoucher_M
    {
        public IEnumerable<Currency_M> Currencies { get; set; }
        public IEnumerable<PaymentType_M> PaymentTypes { get; set; }
        public IEnumerable<UserTreasury_M> UserTreasury { get; set; }
        public string MaxVoucherNo { get; set; }
        public int CurrencyFractionCount { get; set; }
        public IEnumerable<Delegate_M> delegates { get; set; }
        public IEnumerable<Settings_M> settingList { get; set; }
        public dynamic taxAccountObj { get; set; }
        public class Currency_M
        {
            public int id { get; set; }
            public string name { get; set; }
            public int fractionCount { get; set; }
        }

        public class PaymentType_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class UserTreasury_M
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int CurrencyId { get; set; }
            public string notes { get; set; }
            public int cash_account_id { get; set; }
            public int check_account_id { get; set; }
        }

        public class Delegate_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
 public static async Task<InvoiceVoucher_M> LoadInvoiceData(
 IDbConnection db, IDbTransaction trans, 
    int userId,
    int currencyId,
    string type)
{
    try
    {
        // 1) Settings
        List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);

        // 2) Currencies
        var currencies = await db.QueryAsync<Currency_M>(
            "SELECT id, name, CHAR_LENGTH(units) AS fractionCount FROM Currency ORDER BY id ASC",
            transaction: trans);

        // 3) Payment Types
        var paymentTypes = await db.QueryAsync<PaymentType_M>(
            "SELECT id, name FROM Payment_Types ORDER BY name ASC",
            transaction: trans);

        // 4) User Treasury
        var userTreasury = await db.QueryAsync<UserTreasury_M>(
            @"SELECT id, user_id AS UserId, currency_id AS CurrencyId, cash_account_id, check_account_id, notes
              FROM Users_Treasury_Rights 
              WHERE user_id = @userId AND currency_id = @currencyId",
            new { userId, currencyId },
            transaction: trans);

        // 5) Max Voucher Number
        string sql = type == "V"
            ? "SELECT IFNULL(MAX(no), 0) FROM Internal_Consingment"
            : "SELECT IFNULL(MAX(no), 0) FROM Vouchers_And_Bills WHERE type = @type";

        var maxVoucherNo = await db.ExecuteScalarAsync<string>(
            sql,
            new { type },
            transaction: trans);

              if(maxVoucherNo=="0")maxVoucherNo = type + "00000000";

        // 6) Currency Fraction Count
        var units = await db.ExecuteScalarAsync<string>(
            "SELECT units FROM Currency WHERE id = @id",
            new { id = currencyId },
            transaction: trans);

        int fractionCount = !string.IsNullOrEmpty(units) ? units.Length - 1 : 0;

        // 7) Delegates
        var delegats = await db.QueryAsync<Delegate_M>(
            "SELECT id, name FROM Delegates",
            transaction: trans);

        // 8) Copy Currency Exchange Prices
        var _obj = await SHLAPI.Models.Currency.Currency_M.CopyCurrenciesExchangePrice(db, trans, DateTime.Now.Date);

        // 9) VAT Account
        var setting = settingList.FirstOrDefault(s => s.id == 17); // accountSId
        int accountVatId = 0;
        if (setting != null)
            int.TryParse(setting.value, out accountVatId);

        var query = @"
            SELECT c.*, cp.exchange_price
            FROM ChartOfAccount c
            INNER JOIN Currency_prices cp ON c.currency_id = cp.currency_id
            WHERE cp.date = @Date
              AND c.id = @AccountId
        ";

        var taxAccountObj = (await db.QueryAsync<dynamic>(
            query,
            new { Date = DateTime.Now.Date, AccountId = accountVatId },
            transaction: trans
        )).FirstOrDefault();

        return new InvoiceVoucher_M
        {
            Currencies = currencies,
            PaymentTypes = paymentTypes,
            UserTreasury = userTreasury,
            MaxVoucherNo = maxVoucherNo,
            CurrencyFractionCount = fractionCount,
            delegates = delegats,
            taxAccountObj = taxAccountObj
        };
    }
    catch (Exception ex)
    {
        // optionally log error
        return null;
    }
}


        public static async Task<decimal> GetUnitPriceIfCustomerHasSellingWithCost(IDbConnection db, IDbTransaction trans, int itemId)
        {
            try
            {
                int _itemId = itemId;

                string _filter = "1=1  and (Items_and_services.id=" + _itemId + ")"; //MLHIDE
                string transDate = null;
                transDate += "'" + InvoiceVoucherGetData_M.ConvertToSeverDateTimeFormateString(DateTime.Now) + " 23:59:59'";
                List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);
                var setting = settingList.FirstOrDefault(s => s.id == 126); // MethodToCalcCost
                int methodToCalcCost = 1;
                if (setting != null)
                    int.TryParse(setting.value, out methodToCalcCost);
                int UnitPriceFilter = methodToCalcCost;
                if (UnitPriceFilter == 0) UnitPriceFilter = 1;
                else if (UnitPriceFilter == 1) UnitPriceFilter = 3;


                // تنفيذ SP
                var res = await db.QueryAsync<dynamic>(
                    "Items_and_services_StockAssessmentByDate_ForOneItem_GetAllByWhere_sp",
                    new
                    {
                        where = $" 1=1 AND (Items_and_services.id={itemId})",
                        transDate,
                        UnitPriceFilter = UnitPriceFilter.ToString(),
                        itemId = itemId.ToString()
                    },
                    transaction: trans,
                    commandType: CommandType.StoredProcedure
                );

                var firstRow = res.FirstOrDefault();
                if (firstRow == null || firstRow.unitprice == null)
                    return 0;

                decimal.TryParse(firstRow.unitprice.ToString(), out decimal val);

                // ضريبة VAT
                setting = settingList.FirstOrDefault(s => s.id == 16); // VAT
                double vatValue = 0;
                if (setting != null)
                    double.TryParse(setting.value, out vatValue);
                vatValue = (vatValue / 100) + 1;
                val = val * (decimal)vatValue;
                return val;
            }
            catch (Exception EX)
            {
                throw;
            }
        }

        public static async Task<bool> CheckManualNo(IDbConnection db, IDbTransaction trans, string manualNo, string type)
        {
            try
            {
                var query = @"
                        SELECT 1
                        FROM Vouchers_And_Bills
                        WHERE Manual_Voucher_No = @MVNo 
                        AND Type = @Type
                        AND Status = 0";
                var result = await db.QueryFirstOrDefaultAsync<int>(
                    query,
                    new { MVNo = manualNo, Type = type },
                    transaction: trans
                );

                return result == 0;
            }
            catch (Exception EX)
            {
                throw;
            }
        }


    }

    public class InvoiceVoucherGetData_M
    {
        public IEnumerable<dynamic> voucherAll { get; set; }
        public IEnumerable<dynamic> journals { get; set; }
        public IEnumerable<dynamic> chartOfAccount { get; set; }
        public IEnumerable<dynamic> usersTresuryAccountInformations { get; set; }
        public IEnumerable<dynamic> checks { get; set; }
        public IEnumerable<dynamic> cashAccount { get; set; }
        public IEnumerable<dynamic> checkAccount { get; set; }
        public IEnumerable<dynamic> delegateObj { get; set; }
        public IEnumerable<dynamic> checkTransactions { get; set; }
        public IEnumerable<dynamic> employee { get; set; }
        public IEnumerable<dynamic> custSupplier { get; set; }
        public static async Task<InvoiceVoucherGetData_M> GetData(IDbConnection db, IDbTransaction trans, bool _getMaxNoFromService, int voucherId, int userId,
                                                                   string _type, DateTime _date, string voucherNo, string _viewName)
        {

            var resultObj = new InvoiceVoucherGetData_M();
            List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);
            if (_getMaxNoFromService)
            {
                string _voucherMaxNo = "";
                if (voucherNo != "")
                    _voucherMaxNo = voucherNo;
                else
                    _voucherMaxNo = await GetMaxVoucherNOAsync(_type, db, userId, _date, settingList, trans);
                if (voucherId == 0)
                {
                    string _fillter = " no ='" + _voucherMaxNo + "'";
                    List<VoucherAndBillDto> voucherObj = await GetVoucherAndBillsAsync(db, _fillter, userId, trans);
                    if (voucherObj == null) return null;
                    if (voucherObj.Count > 0)
                    {
                        voucherId = (int)voucherObj[0].Id;
                    }
                    if (voucherId == 0)
                    {
                        return null;
                    }
                }
            }


            // DataTable dt = DataLayer.Universal.GetData(con, DataLayer.Universal.StoreProcedures.dynamic_search_sp, false, null, userId, _viewName, "where V_id='" + voucherId + "'", "Row_Number() over (order by V_id) as #, *");
            // dt.TableName = "dtVoucherAll";
            var dtVoucherAll = await GetByDynamicSearchSp(db, _viewName, "where V_id='" + voucherId + "'", "Row_Number() over (order by V_id) as #, *", trans);
            resultObj.voucherAll = dtVoucherAll;
            if (dtVoucherAll == null) return resultObj;
            var person_account_id = dtVoucherAll.First().person_account_id;


            // DataTable dtJournals = BusinessLayer.Journals.GetData(con, Journals.StoreProcedures.Jornals_Rows_sp, false, null, userId, " voucher_id='" + voucherId + "' order by  id,account_serial");
            // dtJournals.TableName = "dtJournals";
            var dtJournals = await GetJornalsRowsSp(db, " voucher_id='" + voucherId + "' order by  id,account_serial", trans);
            resultObj.journals = dtJournals;
            int _cashAccountId = 0;
            if (person_account_id == null || person_account_id + "" == "")
                _cashAccountId = 0;
            else _cashAccountId = (int)person_account_id;

            // DataTable dtChartOfAccount = BusinessLayer.ChartOfAccount.GetAll(con, "id =" + _cashAccountId + "", false, null, userId);
            // dtChartOfAccount.TableName = "dtChartOfAccount";
            var dtChartOfAccount = await GetChartOfAccountSp(db, "id =" + _cashAccountId + "", trans);
            resultObj.chartOfAccount = dtChartOfAccount;
            //DataTable dtGetAllUsersTresuryAccountInfornamtions = DataLayer.Universal.GetAllUsersTresuryAccountInfornamtions(userId, _date);
            //DataTable dtGetAllUsersTresuryAccountInfornamtionsCopy = dtGetAllUsersTresuryAccountInfornamtions.Clone();
            var dtGetAllUsersTresuryAccountInfornamtionsMain = await GetAllUsersTresuryAccountInfornamtions(db, _date, trans);



            int _userIDToGet = userId;
            int _val = 0;
            var entry_user_id = dtVoucherAll.First().entry_user_id;
            var Curr_id = dtVoucherAll.First().Curr_id;
            int.TryParse(entry_user_id + "", out _val);
            if (_val > 0) _userIDToGet = _val;

            var dtGetAllUsersTresuryAccountInfornamtions = dtGetAllUsersTresuryAccountInfornamtionsMain.Where(x => x.user_id == _userIDToGet && x.currency_id == Curr_id).ToList();
            resultObj.usersTresuryAccountInformations = dtGetAllUsersTresuryAccountInfornamtions;
            var dtChecks = await GetByDynamicSearchSp(db, "AllChecksQabdAndSarf_v", " where voucher_id='" + voucherId + "'", "Row_Number() OVER (ORDER BY Check_id ASC) AS #,*", trans);
            resultObj.checks = dtChecks;
            int transID = voucherId;



            #region Get Cash and Checks informations
            int _cashBoxId = 0; int _checkBoxId = 0;
            int _creditCardId = 0;
            foreach (var row in dtJournals)
            {
                string serial = row.account_serial?.ToString();
                string accountIdStr = row.account_id?.ToString();

                if (!string.IsNullOrEmpty(accountIdStr))
                {
                    int accountId = Convert.ToInt32(accountIdStr);

                    switch (serial)
                    {
                        case "-1": // نقدي
                            _cashBoxId = accountId;
                            break;

                        case "1": // شيكات
                            _checkBoxId = accountId;
                            break;
                    }
                }
            }

            var dt_cashAccount = await GetChartOfAccountSp(db, "id =" + _cashBoxId + "", trans);
            resultObj.cashAccount = dt_cashAccount;
            var dt_checkAccount = await GetChartOfAccountSp(db, "id =" + _checkBoxId + "", trans);
            resultObj.checkAccount = dt_checkAccount;
            #endregion

            string _code = "C";
            var code = dtChartOfAccount.First().code;
            if (_code == "E")
            {
                var employee = await GetEmployeeSp(db, " coa_id=" + _cashAccountId + "", trans);
                resultObj.employee = employee;
            }
            else
            {
                var custSupplier = await GetCustsSuppliersSp(db, " coa_id=" + _cashAccountId + "", trans);
                resultObj.custSupplier = custSupplier;
            }

            var _delId = dtVoucherAll.First().delegate_id;
            var dt_Delegate = await GetDelegatesSp(db, " id=" + (_delId == null ? "-1" : _delId + ""), trans);
            resultObj.delegateObj = dt_Delegate;
            var voucherNoVal = dtVoucherAll.First().no;
            var dt_CheckTrans = await CheckTransSp(db, " voucher_no='" + voucherNoVal + "'", trans);
            resultObj.checkTransactions = dt_CheckTrans;
            return resultObj;
        }

        public static async Task<IEnumerable<dynamic>> GetByDynamicSearchSp(IDbConnection db, string _viewName, string whereStatment, string selectStat, IDbTransaction trans)
        {
            try
            {
                var result = await db.QueryAsync<dynamic>(
                                "dynamic_search_sp",
                                new { Str1 = _viewName, Str2 = whereStatment, Str3 = selectStat },
                                commandType: CommandType.StoredProcedure,
                                transaction: trans
                                );
                var list = result.ToList();
                return list;
            }
            catch (Exception EX)
            {
                return null;
            }
        }
        public static async Task<IEnumerable<dynamic>> GetJornalsRowsSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "Jornals_Rows_sp",
                            new { Str1 = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> GetChartOfAccountSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "sp_ChartOfAccount_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> GetAllUsersTresuryAccountInfornamtions(IDbConnection db, DateTime _date, IDbTransaction trans)
        {
            string _select = "SELECT ChartOfAccount.no AS coa_no, ChartOfAccount.name AS coa_name,ChartOfAccount.currency_id AS coa_currencyID,Currency.name";
            _select += " AS currency_name,Users_Treasury_Rights.user_id, Users_Treasury_Rights.currency_id, ChartOfAccount.id AS coa_id,Currency_prices.exchange_price,";
            _select += " Currency_prices.date,ChartOfAccount.stop_transactions  FROM Currency INNER JOIN ChartOfAccount ON Currency.id = ChartOfAccount.currency_id ";
            _select += " RIGHT OUTER JOIN Users_Treasury_Rights ON ChartOfAccount.id = Users_Treasury_Rights.cash_account_id left join Currency_prices on Currency_prices.currency_id = ChartOfAccount.currency_id";

            var result = await db.QueryAsync<dynamic>(
                            "dynamic_search_bySelectStatment_sp",
                            new { Str1 = _select, Str2 = " where Currency_prices.date='" + ConvertToSeverDateTimeFormateString(_date) + "'" },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> GetEmployeeSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "sp_Employees_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> GetCustsSuppliersSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "sp_Custs_Suppliers_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> GetDelegatesSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "sp_Delegates_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> CheckTransSp(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            var result = await db.QueryAsync<dynamic>(
                            "sp_Check_Trans_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }
        public static async Task<IEnumerable<dynamic>> CheckIfHaveCreditCard(IDbConnection db, string whereStat, IDbTransaction trans)
        {
            // مؤجل لاتخاذ القرار
            // DataTable dt_CreditCardVoucher = BusinessLayer.Vouchers_And_Bills.GetAll(con, " other_voucher_no ='" + voucherId + "'", false, null, UserId);
            var result = await db.QueryAsync<dynamic>(
                            "sp_Check_Trans_GetAllByWhere",
                            new { where = whereStat },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }

        public static async Task<dynamic> GetAccountAllData(IDbConnection db, int coa_id, IDbTransaction trans)
        {
            try
            {
                string where = string.Format(" id ={0}", coa_id);
                var result = await db.QueryFirstOrDefaultAsync<dynamic>(
         "sp_ChartOfAccount_GetAllByWhere",
         new { where },
         commandType: CommandType.StoredProcedure,
         transaction: trans
     );
                return result;
            }
            catch (Exception EX)
            {
                throw;
            }

        }

        public static async Task<bool> CheckIfHaveManualVoucherNo(IDbConnection db, IDbTransaction trans, string manualVoucherNo, string _type)
        {
            string _fillter = " Manual_Voucher_No='" + manualVoucherNo + "' and type='" + _type + "' and status = 0";
            var result = await db.QueryAsync<dynamic>(
                            "sp_Vouchers_And_Bills_GetAllByWhere",
                            new { where = _fillter },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            if (list == null) return false;
            if (list.AsList().Count > 0)
            {
                return true;
            }
            return false;
        }



        public static async Task<IEnumerable<dynamic>> GetCostCenters(IDbConnection db, int userId, IDbTransaction trans)
        {
            bool onlySelected = false;
               string sql = @"
    SELECT DISTINCT 
        IF(uc.cost_center_id IS NULL, 0, uc.permission) AS permission,
        cc.id AS cost_center_id,
        cc.code,
        cc.name,
        IF(uc.is_defualt IS NULL, 0, uc.is_defualt) AS is_default
    FROM Cost_Center cc
    LEFT JOIN User_CostCenter uc 
        ON cc.id = uc.cost_center_id 
        AND uc.user_id = @userId
    WHERE (cc.State = '0' OR cc.State IS NULL)
    ";

    if (onlySelected)
        sql += " AND IF(uc.permission IS NULL, 0, uc.permission) = 1";

    return await db.QueryAsync<dynamic>(sql, new { userId },trans);
        }

        public static async Task<IEnumerable<dynamic>> GetStoresOld(IDbConnection db, int userId, IDbTransaction trans)
        {
            string where = userId + "";
            var result = await db.QueryAsync<dynamic>(
                            "GetAllSelectedUserStores_sp",
                                   new
                                   {
                                       Str1 = where,
                                       Str2 = "1"
                                   },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }

        public static async Task<IEnumerable<dynamic>> GetStores(
   IDbConnection db, int userId, IDbTransaction trans,
    bool onlyTruePermissions = false) // corresponds to Str2
{
    try
    {
        // Base query
        string sql = @"
            SELECT 
                CASE 
                    WHEN us.store_id IS NULL THEN TRUE
                    ELSE us.permission 
                END AS permission,
                s.id AS stores_id,
                s.name
            FROM Stores s
            LEFT JOIN Users_Stores us 
                ON s.id = us.store_id 
                AND us.user_id = @UserId
            WHERE s.status = 0";

        // Add filter for only TRUE permissions if requested (Str2 was not empty)
        if (onlyTruePermissions)
        {
            sql += " AND (us.store_id IS NULL OR us.permission = TRUE)";
        }

        var res = await db.QueryAsync<dynamic>(
            sql,
            new { UserId = userId },
            transaction: trans
        );

        return res;
    }
    catch (Exception ex)
    {
        throw;
    }
}




        public static async Task<IEnumerable<dynamic>> CheckCostCenterActive(IDbConnection db, int id, IDbTransaction trans)
        {
            string where = "  id=" + id;
            var result = await db.QueryAsync<dynamic>(
                            "sp_Cost_Center_GetAllByWhere",
                                   new
                                   {
                                       where = where
                                   },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }

        public static async Task<IEnumerable<dynamic>> CheckStoreActive(IDbConnection db, int id, IDbTransaction trans)
        {
            string where = "  id=" + id;
            var result = await db.QueryAsync<dynamic>(
                            "sp_Stores_GetAllByWhere",
                                   new
                                   {
                                       where = where
                                   },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }


        public static string ConvertToSeverDateTimeFormateString(DateTime date)
        {

            // إذا بدك التاريخ فقط
           // return date.ToString("MM-dd-yyyy");
           
             return date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            // أو إذا بدك التاريخ مع الوقت
            // return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public class VoucherAndBillDto
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string No { get; set; }
            public DateTime? Date { get; set; }
            public int? PersonAccountId { get; set; }
            public string PersonName { get; set; }
            public string ManualVoucherNo { get; set; }
            public int? OtherVoucherNo { get; set; }
            public int? CurrId { get; set; }
            public decimal? TotalAmount { get; set; }
            public decimal? CashAmount { get; set; }
            public decimal? CheckAmount { get; set; }
            public int? DelegateId { get; set; }
            public int? PaymentTypeId { get; set; }
            public string Notes { get; set; }
            public bool? Printed { get; set; }
            public bool? InvoiceIssued { get; set; }
            public string Status { get; set; }
            public decimal? DeductionPercentage { get; set; }
            public decimal? Vat { get; set; }
            public string BillTaxType { get; set; }
            public string MaqassahBill { get; set; }
            public bool? PrintWithTax { get; set; }
            public decimal? DeductionAmount { get; set; }
            public int? EntryUserId { get; set; }
            public DateTime? EntryDate { get; set; }
            public decimal? ExchangePrice { get; set; }
            public string Tagged { get; set; }
            public string CustomerCreditCardNo { get; set; }
            public int? BankId { get; set; }
            public int? BranchId { get; set; }
            public bool? GenaratedBySystem { get; set; }
            public string AccountPrintName { get; set; }
            public int? RefrenceId { get; set; }
            public int? OrderTypeId { get; set; }
            public int? TableId { get; set; }
            public int? DriverId { get; set; }
            public DateTime? StartDate { get; set; }
            public TimeSpan? PayTime { get; set; }
            public int? StoreId { get; set; }
            public string StoreName { get; set; }
            public int? InternalConsingmentId { get; set; }
            public int? WaiterId { get; set; }
            public DateTime? DueDate { get; set; }
            public int? BatchId { get; set; }
            public int? GenaratedBySystemTypeId { get; set; }
            public string VatReg { get; set; }
            public string VatRegCustSupp { get; set; }
            public int? TransYear { get; set; }
            public string DiscountType { get; set; }
            public int? ConnectedId { get; set; }
            public int? QueueNo { get; set; }
            public string QueueStatus { get; set; }
            public DateTime? QueuStatuTime { get; set; }
        }
        public static async Task<List<VoucherAndBillDto>> GetVoucherAndBillsAsync(IDbConnection con, string filter, int userId, IDbTransaction trans)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@where", filter);

                // افترض أن SP اسمها sp_Vouchers_And_Bills_GetAllByWhere
                var result = await con.QueryAsync<VoucherAndBillDto>(
                    "sp_Vouchers_And_Bills_GetAllByWhere",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: trans
                );
                return result.ToList();
            }
            catch (Exception EX)
            {
                return null;
            }

        }
        public static async Task<string> GetMaxVoucherNOAsync(
        string type,
        IDbConnection con,
        int userId,
        DateTime date,
        List<Settings_M> settingList,
        IDbTransaction trans = null)
        {
            string voucherNO = "";

            try
            {
                var setting = settingList.FirstOrDefault(s => s.id == 178); // RebaseTransNo
                bool rebaseTransNo = false;
                if (setting != null)
                    bool.TryParse(setting.value, out rebaseTransNo);
                if (type == "V")
                {
                    string where = "";

                    if (rebaseTransNo)
                    {
                        int transYear = int.Parse(date.ToString("yy"));
                        where += " where trans_year = " + transYear;
                    }

                    // استعلام Internal_Consingment
                    var sql = $"select max(no) from Internal_Consingment {where}";
                    voucherNO = await con.ExecuteScalarAsync<string>(sql, transaction: trans);
                }
                else
                {
                    string where = $" type = '{type}'";
                    if (rebaseTransNo)
                    {
                        int transYear = int.Parse(date.ToString("yy"));
                        where += $" and trans_year = {transYear}";
                    }

                    // استدعاء Stored Procedure MaxVouchers_And_Bills_sp
                    var parameters = new DynamicParameters();
                    parameters.Add("@where", where);

                    voucherNO = await con.ExecuteScalarAsync<string>(
                        "MaxVouchers_And_Bills_sp",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: trans
                    );
                }
            }
            catch
            {
                return "";
            }

            return voucherNO ?? "";
        }
        public enum navigateTypeEnum
        {
            next = 1,
            prev = 2
        }
        public static async Task<IEnumerable<dynamic>> Navigate(IDbConnection db, IDbTransaction trans, int voucherId, navigateTypeEnum navigateType, string voucherType)
        {
            try
            {
                string where = "";
                if (voucherId == 0)
                {
                    if (navigateType == navigateTypeEnum.next)
                    {
                        where = string.Format(" id= (select min(id) from dbo.Vouchers_And_Bills where type='{0}') ", voucherType);
                    }
                    if (navigateType == navigateTypeEnum.prev)
                    {
                        where += string.Format(" id= (select max(id) from dbo.Vouchers_And_Bills where type='{0}') ", voucherType);
                    }
                }
                else
                {
                    if (navigateType == navigateTypeEnum.next)
                        where += string.Format(" id= (select min(id) from dbo.Vouchers_And_Bills where id > {0} and type='{1}') ", voucherId, voucherType);

                    else if (navigateType == navigateTypeEnum.prev)
                        where += string.Format(" id= (select max(id) from dbo.Vouchers_And_Bills where id < {0} and type='{1}') ", voucherId, voucherType);
                }
                var result = await db.QueryAsync<dynamic>(
                                "sp_Vouchers_And_Bills_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                return list;
            }
            catch (Exception EX)
            {
                throw;
            }
        }

        /////////////////// check when delete 
        public static async Task<bool> CheckVoucherHaveBillPay(IDbConnection db, IDbTransaction trans, int voucherId)
        {
            try
            {
                string where = " voucher_id=" + voucherId + "";
                var result = await db.QueryAsync<dynamic>(
                                "sp_Bills_Pay_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list != null && list.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        public class VoucherResult
        {
            public bool result { get; set; }
            public string msg { get; set; }
            public string value { get; set; }
        }
        public static async Task<VoucherResult> DeleteQabdSarfVoucher(IDbConnection db, IDbTransaction trans, int voucherId, int userId, string _type,
                                                      DateTime _date, string voucherNo, string _viewName, string deletedNote)
        {
            VoucherResult returnResult = new VoucherResult();
            returnResult.result = true;
            returnResult.msg = "";
            InvoiceVoucherGetData_M voucherObj = await GetData(db, trans, false, voucherId, userId, _type, _date, voucherNo, _viewName);

            var voucherSaveObj = voucherObj.voucherAll.First();
            if (voucherObj != null && voucherObj.voucherAll != null && voucherObj.voucherAll.AsList().Count > 0)
            {
                var voucherType = voucherSaveObj.type;
                if (voucherType == "R" || voucherType == "P")
                {
                    bool voucherHaveChecks = await IfVoucherHaveChecks(db, trans, voucherId);
                    if (voucherHaveChecks)
                    {
                        returnResult.result = false;
                        returnResult.msg = "haveChecksTrans";
                        return returnResult;
                    }
                }
            }

            try
            {

                string updateQuery = @"
                    UPDATE Vouchers_And_Bills
                    SET 
                        Status = '1',Deleted_Date=@deletedDate,Notes=@deletedNote
                    WHERE id = @id;
                ";
                int rows = await db.ExecuteAsync(updateQuery, new
                {
                    id = voucherId,
                    deletedDate = DateTime.Now,
                    deletedNote
                }, trans);


                updateQuery = @"
                    UPDATE Journals
                    SET 
                        Status = '1' 
                    WHERE voucher_id = @id;
                ";

                int rowsJ = await db.ExecuteAsync(updateQuery, new
                {
                    id = voucherId
                }, trans);

                updateQuery = @"
                    UPDATE Checks
                    SET 
                        Status = '1' 
                    WHERE voucher_id = @id;
                ";

                int rowsChk = await db.ExecuteAsync(updateQuery, new
                {
                    id = voucherId
                }, trans);

                updateQuery = @"
                    update  Check_Trans  set Check_Trans.status ='1'
                    where Check_Trans.id in(select Check_Trans.id from Check_Trans
                    inner join Checks on Checks.id = Check_Trans.check_id 
                    where Checks.voucher_id = @voucherId)
                ";

                int rowsChkTrans = await db.ExecuteAsync(updateQuery, new
                {
                    voucherId
                }, trans);

                if (!string.IsNullOrEmpty(voucherSaveObj.customer_credit_card_no))
                {
                    returnResult.result = false;
                    returnResult.msg = "haveCardTrans"; //"لا يمكن حذف قبض عليه تسديدات بواسطة بطاقة ائتمان";
                    return returnResult;
                }

                updateQuery = @"
                    UPDATE Bills_Pay
                    SET 
                        Status = '1' 
                    WHERE voucher_id = @voucherId;
                ";

                int rowsBillPay = await db.ExecuteAsync(updateQuery, new
                {
                    voucherId
                }, trans);

            }
            catch (Exception EX)
            {
                throw;
            }
            return returnResult;
        }

        public static async Task<bool> IfVoucherHaveChecks(IDbConnection db, IDbTransaction trans, int voucherId)
        {
            try
            {
                string where = string.Format(" voucher_id={0}", voucherId);
                var result = await db.QueryAsync<dynamic>(
                                "sp_Checks_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list != null && list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {

                        var _checkId = list[i].id;
                        string whereChecks = string.Format(" check_id={0} and operation_id <> -1 and status='0' ", _checkId);
                        var resultChecks = await db.QueryAsync<dynamic>(
                                        "sp_Check_Trans_GetAllByWhere",
                                        new { where = whereChecks },
                                        commandType: CommandType.StoredProcedure,
                                            transaction: trans
                                        );
                        var listChecks = resultChecks.ToList();
                        if (listChecks != null && listChecks.Count > 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        /////////////////// end check when delete 

        //////////////////////////// Save 


        public class FullVoucher
        {
            public Voucher Voucher { get; set; }
            public List<Journal> Journals { get; set; }
            public List<VouchersItemsAndServices> vouchersItemsAndServices { get; set; }
        }

        // أمثلة على DTO لكل كائن
        public class Voucher
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string No { get; set; }
            public DateTime Date__ { get; set; }
            public int Person_Account_Id { get; set; }
            public string Person_Name { get; set; }
            public string Manual_Voucher_No { get; set; }
            public string Other_Voucher_No { get; set; }
            public int Curr_Id { get; set; }
            public double Total_Amount { get; set; }
            public double Cash_Amount { get; set; }
            public double Check_Amount { get; set; }
            public int Delegate_Id { get; set; }
            public int Payment_Type_Id { get; set; }
            public string Notes { get; set; }
            public bool Printed { get; set; }
            public bool Invoice_Issued { get; set; }
            public string Status { get; set; }
            public double Deduction_Percentage { get; set; }
            public double Vat { get; set; }
            public string Bill_Tax_Type { get; set; }
            public bool Maqassah_Bill { get; set; }
            public bool Print_With_Tax { get; set; }
            public double Deduction_Amount { get; set; }
            public int Entry_User_Id { get; set; }
            public DateTime Entry_Date { get; set; }
            public double Exchange_Price { get; set; }
            public string Tagged { get; set; }
            public string Customer_Credit_Card_No { get; set; }
            public int Bank_Id { get; set; }
            public int Branch_Id { get; set; }
            public bool Genarated_By_System { get; set; }
            public string Account_Print_Name { get; set; }
            public int Refrence_Id { get; set; }
            public int Order_Type_Id { get; set; }
            public int Table_Id { get; set; }
            public int Driver_Id { get; set; }
            public DateTime Start_Date { get; set; }
            public int Pay_Time { get; set; }
            public int Store_Id { get; set; }
            public string Store_Name { get; set; }
            public int Internal_Consingment_Id { get; set; }
            public int Waiter_Id { get; set; }
            public DateTime Due_Date { get; set; }
            public int Batch_Id { get; set; }
            public int Genarated_By_System_Type_Id { get; set; }
            public string Vat_Reg { get; set; }
            public string Vat_Reg_Cust_Supp { get; set; }
            public int Trans_Year { get; set; }
            public int Discount_Type { get; set; }
            public int Connected_Id { get; set; }
            public int Queue_No { get; set; }
            public int Queue_Status { get; set; }
            public DateTime? Queu_Statu_Time { get; set; }
            public DateTime? Deleted_Date { get; set; }
        }

        public class Journal
        {
            public string Debit_Credit { get; set; }
            public int Account_Serial { get; set; }
            public int Account_Id { get; set; }
            public int Cost_Center_Id { get; set; }
            public string Cost_Center_Name { get; set; }
            public double Actual_Amount { get; set; }
            public int Actual_Amount_Curr { get; set; }
            public double Account_Equal_Amount { get; set; }
            public int Account_Curr { get; set; }
            public double Base_Curr_Amount { get; set; }
            public double Equal_Exchange_Price { get; set; }
            public double Exchange_Price { get; set; }
            public string Status { get; set; }
            public int Voucher_Id { get; set; }
            public int Is_Tax_Account { get; set; }
            public string Notes { get; set; }
        }
        public class VouchersItemsAndServices
        {
            public int id { get; set; }
            public int voucher_id { get; set; }
            public int item_service_id { get; set; }
            public int item_unit_id { get; set; }
            public int main_unit_id { get; set; }
            public DateTime qty_expire_date { get; set; }
            public double quantity { get; set; }
            public double bonus_qty { get; set; }
            public double unit_price { get; set; }
            public string notes { get; set; }
            public double to_main_unit_qty { get; set; }
            public double base_exchange_price { get; set; }
            public double unit_price_with_discount_and_tax { get; set; }
            public double cost { get; set; }
            public double item_discount_amount { get; set; }
            public string item_print_name { get; set; }
            public int waiter_id { get; set; }
            public double item_vat { get; set; }
            public bool is_custody { get; set; }
            public bool is_pos_fixed { get; set; }
            public int branch_id { get; set; }
            public int offer_price_id { get; set; }
            public int campaign_id { get; set; }
            public int camp_item_type { get; set; }
            public string item_unit_bar_code { get; set; }
        }
        public static async Task<VoucherResult> SaveInvoiceVoucher(IDbConnection db, IDbTransaction trans, SaveInvoiceVoucherF.Query obj)
        {
            VoucherResult returnObj = new VoucherResult();
            returnObj.result = true;
            try
            {
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (obj.voucherObj.Delegate_Id > 0)
                {
                    var dt_Delegate = await GetDelegatesSp(db, " id=" + (obj.voucherObj.Delegate_Id == 0 ? "-1" : obj.voucherObj.Delegate_Id + ""), trans);
                    if (dt_Delegate != null && dt_Delegate.AsList().Count > 0 && dt_Delegate.AsList()[0].State == "1")
                    {
                        returnObj.result = false;
                        returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.DelegateFreez + "";
                        return returnObj;
                    }
                }
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (obj.voucherObj.Store_Id > 0)
                {
                    var dt_Stores = await CheckStoreActive(db, obj.voucherObj.Store_Id, trans);
                    if (dt_Stores != null && dt_Stores.AsList().Count > 0 && dt_Stores.AsList()[0].Status == "1")
                    {
                        returnObj.result = false;
                        returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.StoresIsSttopped + "";
                        return returnObj;
                    }
                }
                List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);
                string _voucherMaxNo = await GetMaxVoucherNOAsync(obj.voucherObj.Type, db, obj.user_id, obj.voucherObj.Date__, settingList, trans);
                string _maxNoPlusOne = obj.voucherObj.Type + Inc_Code(_voucherMaxNo, obj.voucherObj.Date__, settingList);
                ///////////////////////////////////////////////////////////////////////////////////////////
                obj.voucherObj.No = _maxNoPlusOne;
                #region Set Printed = true if manual voucher id != ""
                var setting = settingList.FirstOrDefault(s => s.id == 130); // NotPrintOriginalWithMVNo
                bool notPrintOriginalWithMVNo = false;
                if (setting != null)
                    bool.TryParse(setting.value, out notPrintOriginalWithMVNo);
                if (obj.voucherObj.Manual_Voucher_No != "" && notPrintOriginalWithMVNo)
                {
                    obj.voucherObj.Printed = true;
                }
                #endregion




                obj.voucherObj.Entry_Date = DateTime.Now;
                obj.voucherObj.Entry_User_Id = obj.user_id;
                if (obj.voucherObj.Type != obj.voucherObj.No.ToCharArray()[0].ToString())
                {
                    obj.voucherObj.Type = obj.voucherObj.No.ToCharArray()[0].ToString();
                }
                int saveVoucher = await InsertVoucher(db, trans, obj.voucherObj);
                obj.voucherObj.Id = saveVoucher;
                if (saveVoucher <= 0)
                {
                    returnObj.result = false;
                    returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.Error + "";
                    return returnObj;
                }
                double _childAccountEqualAmount = 0;
                foreach (Journal journalObj in obj.journalList)
                {
                    if (journalObj.Account_Serial == -1)
                    {
                        _childAccountEqualAmount = journalObj.Account_Equal_Amount;
                        if (journalObj.Cost_Center_Id > 0)
                        {
                            var dt_CostCenter = await CheckCostCenterActive(db, journalObj.Cost_Center_Id, trans);
                            if (dt_CostCenter != null && dt_CostCenter.AsList().Count > 0 && dt_CostCenter.AsList()[0].State == "1")
                            {
                                returnObj.result = false;
                                returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.CostCenterStopped + "";
                                return returnObj;
                            }
                        }
                    }
                    journalObj.Voucher_Id = obj.voucherObj.Id;

                    bool _ifAccountIsFather = await CheckIfHasFather(db, trans, journalObj.Account_Id);

                    #region CheckCostCenter Stopped
                    if (journalObj.Cost_Center_Id > 0)
                    {
                        bool checkCostCenter = await CheckCostCenter(db, trans, journalObj.Cost_Center_Id);
                        if (!checkCostCenter)
                        {
                            returnObj.result = false;
                            returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.CostCenterStopped + "";
                            return returnObj;
                        }
                    }
                    #endregion
                    int journalSave = await InsertJournal(db, trans, journalObj);
                    if (_ifAccountIsFather || journalSave <= 0)
                    {
                        returnObj.result = false;
                        returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.IsFather + "";
                        return returnObj;
                    }
                }

                if (obj.vouchersItemsAndServicesList != null)
                {

                    foreach (VouchersItemsAndServices voucherItem in obj.vouchersItemsAndServicesList)
                    {

                        voucherItem.voucher_id = obj.voucherObj.Id;

                        string sql = @"
                        SELECT TOP 1 *
                        FROM dbo.Items_units
                        WHERE item_id = @ItemId and main_sell_unit=1";

                        var result = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { ItemId = voucherItem.item_service_id }, trans);
                        if (result != null)
                            voucherItem.main_unit_id = result.unit_id;
                        int voucherItemRes = await InsertVoucherItem(db, trans, voucherItem, obj);

                        if (voucherItemRes <= 0)
                        {
                            returnObj.result = false;
                            returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.Error + "";
                            return returnObj;
                        }
                    }
                }

                returnObj.result = true;
                returnObj.value = "";
                return returnObj;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        public class CheckConnectedInvoiceClass
        {
            public bool _obj { get; set; }
            public string _no { get; set; }
        }
        public static async Task<CheckConnectedInvoiceClass> CheckConnectedInvoice(
                    IDbConnection db,
                    IDbTransaction trans,
                    int voucherId)
        {
            CheckConnectedInvoiceClass returnRes = new CheckConnectedInvoiceClass();
            returnRes._obj = true;
            try
            {
                string where = string.Format(" connected_id={0} and status =0 ", voucherId);
                var result = await db.QueryAsync<dynamic>(
                                "sp_Vouchers_And_Bills_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list.Count > 0)
                {
                    returnRes._obj = true;
                    returnRes._no = list[0].no;
                }
                else returnRes._obj = false;
                return returnRes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> CheckIfHasFather(IDbConnection db, IDbTransaction trans, int _accountId)
        {
            try
            {
                string where = string.Format(" parent_account_id={0} ", _accountId);
                var result = await db.QueryAsync<dynamic>(
                                "sp_ChartOfAccount_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list.Count > 0)
                {
                    return true;
                }

            }
            catch (Exception)
            {

                return true;
            }
            return false;
        }
        public static async Task<bool> CheckCostCenter(IDbConnection db, IDbTransaction trans, int costCenterId)
        {
            try
            {
                string where = string.Format(" id={0} and State=0", costCenterId);
                var result = await db.QueryAsync<dynamic>(
                                "sp_Cost_Center_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {

                return true;
            }
            return false;
        }
        public static async Task<bool> ChecksIfChecksNotDublicate(IDbConnection db, IDbTransaction trans, string _value, int _userId, string customer_bank_account)
        {
            try
            {
                if (_value == "") return true;
                string where = string.Format(" no='{0}' and customer_bank_account='{1}' and Status='0'", _value, customer_bank_account);
                var result = await db.QueryAsync<dynamic>(
                                "sp_Checks_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list.Count > 0) return true;
                return false;
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        public enum AllDataOnSelectedCmbCurrencyChanged
        {
            DontHaveCashAccountId,
            CashBoxAccountIsStopped,
            DontHaveExchangePrice,
            ErrorUserTresuryForAccount,
            Succssfull,
            SuccssfullWithNotify,
            Error,
            C,
            D,
            E,
            DelegateFreez,
            DuplicatedCheckNumber,
            IsFather,
            ErrorWhenSavePayBill,
            ErrorVoucherNoLength,
            ErrorManualNoLength,
            AccountStopped,
            AccountCurrencyDiffrentWithVoucherCurrency,
            ErrorAccountNameLength,
            ErrorExchangePriceOfMainCurrency,
            ErrorInTotalAmount,
            ErrorInAmountChecks,
            ErrorTotalAmountEqualZero,
            ErrorNotesLength,
            ErrorAccountNoLengthInGrid,
            CostCenterStopped,
            StoresIsSttopped
        }
        public enum Validation_C_D_E
        {
            success, error, E, C, D, T
        }
        public class QabdQuickValidationClass
        {
            public AllDataOnSelectedCmbCurrencyChanged allDataOnSelectedCmbCurrencyChanged { get; set; }
            public string _differentCurrencyTrans { get; set; }
            public int _coaAccountCurrency { get; set; }
            public double _exchangePrice { get; set; }
            public int _getCurrencyFractionCount { get; set; }
        }
        public static async Task<QabdQuickValidationClass> QabdQuickValidation(IDbConnection db, IDbTransaction trans, int currencyId, int _userId, int _customerId,
                                                                                int _debtCurrencyId, DateTime _date, bool _checkNotificationMessage, string _screenType)
        {
            QabdQuickValidationClass QabdQuickValidationClassObj = new QabdQuickValidationClass();
            try
            {
                CheckUserTresuryForAccountResult returnCheckUserTresuryForAccountResult = await CheckUserTresuryForAccount(currencyId, db, trans, _userId, _customerId);
                QabdQuickValidationClassObj._coaAccountCurrency = returnCheckUserTresuryForAccountResult._coaAccountCurrency;
                QabdQuickValidationClassObj._differentCurrencyTrans = returnCheckUserTresuryForAccountResult._differentCurrencyTrans;
                if (!returnCheckUserTresuryForAccountResult.result)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.ErrorUserTresuryForAccount;
                    return QabdQuickValidationClassObj;
                }



                QabdQuickValidationClassObj._getCurrencyFractionCount = await GetCurrencyFractionCount(db, trans, currencyId);
                GetExchangeCurrencyPriceByDateClass getExchangeCurrencyPriceByDate = await GetExchangeCurrencyPriceByDate(_debtCurrencyId, _date, db, true, trans);
                QabdQuickValidationClassObj._exchangePrice = getExchangeCurrencyPriceByDate.ExchangePrice;

                Validation_C_D_E _res = await CheckValidation_C_D_E(db, trans, _date, 0);
                if (_res == Validation_C_D_E.C)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.C;
                    return QabdQuickValidationClassObj;
                }
                else if (_res == Validation_C_D_E.D)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.D;
                    return QabdQuickValidationClassObj;
                }
                else if (_res == Validation_C_D_E.E)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.E;
                    return QabdQuickValidationClassObj;
                }
                else if (_res == Validation_C_D_E.error)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.Error;
                    return QabdQuickValidationClassObj;
                }
                else if (_res == Validation_C_D_E.success)
                {
                    QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.Succssfull;
                    return QabdQuickValidationClassObj;
                }
            }
            catch
            {
                QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.Error;
                return QabdQuickValidationClassObj;
            }
            QabdQuickValidationClassObj.allDataOnSelectedCmbCurrencyChanged = AllDataOnSelectedCmbCurrencyChanged.Succssfull;
            return QabdQuickValidationClassObj;
        }

        public class CheckUserTresuryForAccountResult
        {
            public string _differentCurrencyTrans { get; set; }
            public int _coaAccountCurrency { get; set; }
            public bool result { get; set; }
        }
        private static async Task<CheckUserTresuryForAccountResult> CheckUserTresuryForAccount(int _currId, IDbConnection db, IDbTransaction trans, int _userId, int _customerId)
        {
            CheckUserTresuryForAccountResult returnResult = new CheckUserTresuryForAccountResult();
            try
            {
                if (_customerId != 0)
                {

                    string where = string.Format(" id={0}", _customerId);
                    var result = await db.QueryAsync<dynamic>(
                                    "sp_ChartOfAccount_GetAllByWhere",
                                    new { where },
                                    commandType: CommandType.StoredProcedure,
                                        transaction: trans
                                    );
                    var list = result.ToList();
                    if (list.Count > 0)
                    {
                        if (list[0].currency_id != (int)_currId)
                        {
                            returnResult._differentCurrencyTrans = list[0].different_currency_trans + "";
                            returnResult._coaAccountCurrency = list[0].currency_id;
                            if (list[0].different_currency_trans + "" == "1")
                            {
                                returnResult.result = false;

                                return returnResult;
                            }
                            returnResult.result = true;
                            return returnResult;
                        }
                    }
                    else
                    {
                        returnResult.result = false;
                        return returnResult;
                    }
                }
            }
            catch (Exception EX)
            {
                returnResult.result = false;
                return returnResult;
            }
            returnResult.result = true;
            return returnResult;
        }


        private static async Task<int> GetCurrencyFractionCount(IDbConnection db, IDbTransaction trans, int voucher_curr_id)
        {
            try
            {
                if (voucher_curr_id == 0) return 0;
                string where = string.Format(" id={0} order by id asc", voucher_curr_id);
                var result = await db.QueryAsync<dynamic>(
                                "sp_Currency_GetAllByWhere",
                                new { where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list.Count > 0)
                {
                    return list[0].units.ToString().ToCharArray().Length - 1;
                }
                else return 0;
            }
            catch
            {
                return 3;
            }
        }

        public class GetExchangeCurrencyPriceByDateClass
        {
            public bool result { get; set; }
            public double ExchangePrice { get; set; }
        }
        public static async Task<GetExchangeCurrencyPriceByDateClass> GetExchangeCurrencyPriceByDate(int CurrencyId, DateTime Date, IDbConnection db, bool ThrowException, IDbTransaction trans)
        {

            GetExchangeCurrencyPriceByDateClass returnObj = new GetExchangeCurrencyPriceByDateClass();
            returnObj.ExchangePrice = 0;
            returnObj.result = false;
            // ref double ExchangePrice

       

            string where = string.Format(" where currency_id={0} and date='{1}' ", CurrencyId, ConvertToSeverDateTimeFormateString(Date));
            var result = await db.QueryAsync<dynamic>(
                            "Currency_Price_sp",
                            new { where },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            if (list.Count > 0)
            {
                returnObj.ExchangePrice = list[0].exchange_price;
                returnObj.result = true;
                return returnObj;
            }
            else
            {
                return returnObj;
            }
        }


        public static async Task<Validation_C_D_E> CheckValidation_C_D_E(IDbConnection db, IDbTransaction trans, DateTime _date, int _voucherId)
        {

            try
            {
                string _dateString = ConvertToSeverDateTimeFormateString(_date);

                string _fillter = " date >='" + _dateString + "' and tagged='E' and status ='0' and person_name <> 'قيد اثبات بضاعة اول المدة' ";
                if (_voucherId != 0) _fillter += " and id<>" + _voucherId + " and id > " + _voucherId + "";

                var result = await db.QueryAsync<dynamic>(
                                "sp_Vouchers_And_Bills_GetAllByWhere",
                                new { where = _fillter },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result.ToList();
                if (list == null) return Validation_C_D_E.error;
                if (list.Count > 0)
                {
                    return Validation_C_D_E.E;
                }


                _fillter = " date >='" + _dateString + "' and tagged='D' and status ='0' ";
                if (_voucherId != 0) _fillter += " and id<>" + _voucherId + " and id > " + _voucherId + "";

                result = await db.QueryAsync<dynamic>(
                               "sp_Vouchers_And_Bills_GetAllByWhere",
                               new { where = _fillter },
                               commandType: CommandType.StoredProcedure,
                                   transaction: trans
                               );
                list = result.ToList();
                if (list == null) return Validation_C_D_E.error;
                if (list.Count > 0)
                {
                    return Validation_C_D_E.D;
                }

                _fillter = " date >='" + _dateString + "' and tagged='C' and status ='0' ";
                if (_voucherId != 0) _fillter += " and id<>" + _voucherId + " and id > " + _voucherId + "";

                result = await db.QueryAsync<dynamic>(
                               "sp_Vouchers_And_Bills_GetAllByWhere",
                               new { where = _fillter },
                               commandType: CommandType.StoredProcedure,
                                   transaction: trans
                               );
                list = result.ToList();
                if (list == null) return Validation_C_D_E.error;
                if (list.Count > 0)
                {
                    return Validation_C_D_E.C;
                }

                _fillter = " date >='" + _dateString + "' and genarated_by_system_type_id=6 and status ='0' ";
                if (_voucherId != 0) _fillter += " and id<>" + _voucherId + " and id > " + _voucherId + "";

                result = await db.QueryAsync<dynamic>(
                               "sp_Vouchers_And_Bills_GetAllByWhere",
                               new { where = _fillter },
                               commandType: CommandType.StoredProcedure,
                                   transaction: trans
                               );
                list = result.ToList();
                if (list == null) return Validation_C_D_E.error;
                if (list.Count > 0)
                {
                    return Validation_C_D_E.D;
                }

            }
            catch (Exception EX)
            {
                throw;
            }

            return Validation_C_D_E.success;
        }

        static public string Inc_Code(string code, DateTime voucherDate, List<Settings_M> settingList)
        {
            var setting = settingList.FirstOrDefault(s => s.id == 178); // RebaseTransNo
            bool rebaseTransNo = false;
            if (setting != null)
                bool.TryParse(setting.value, out rebaseTransNo);
            string yearVal = "";
            int year = voucherDate.Year;
            if (year.ToString().ToCharArray().Length > 2)
                yearVal = year.ToString().Remove(0, 2);
            if (code.Trim().Length == 0) return code;
            char codeValue = code[0];
            if (code.Length > 0)
            {
                if (code[0] == 'R') code = code.Remove(0, 1);
                else if (code[0] == 'J') code = code.Remove(0, 1);
                else if (code[0] == 'P') code = code.Remove(0, 1);
                else if (code[0] == 'C') code = code.Remove(0, 1);
                else if (code[0] == 'D') code = code.Remove(0, 1);
                else if (code[0] == 'E') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'I') code = code.Remove(0, 1);
                else if (code[0] == 'A') code = code.Remove(0, 1);
                else if (code[0] == 'H') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'M') code = code.Remove(0, 1);
                else if (code[0] == 'B') code = code.Remove(0, 1);
                else if (code[0] == 'V') code = code.Remove(0, 1);
                else if (code[0] == 'T') code = code.Remove(0, 1);
                else if (code[0] == 'Q') code = code.Remove(0, 1);
                else if (code[0] == 'K') code = code.Remove(0, 1);
            }
            int i;
            if (code == "")
            {
                code = codeValue + "00000000";                        // //MLHIDE
                if (rebaseTransNo)
                    code = codeValue + yearVal + "000000";                        // //MLHIDE
            }
            if (rebaseTransNo)
            {
                code = code.Remove(0, 2);
                code = yearVal + code;
            }
            char[] code2 = code.ToCharArray();
            i = code2.Length - 1;
            while (i > 0 && code2[i] == 32) i--;
            if (code2[i] == '9')
            {
                while (code2[i] == '9' && i > 0)
                {
                    code2[i] = '0';
                    i--;
                }
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            else
            {
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            string newCode = new string(code2);
            return newCode;

        }// Incerement Customer Code...

        public static async Task<int> InsertVoucher(IDbConnection db, IDbTransaction trans, Voucher voucherObj)
        {

            var parameters = new DynamicParameters();
            try
            {
                DateTime minSqlDate = new DateTime(1753, 1, 1);
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@type", voucherObj.Type);
                parameters.Add("@no", voucherObj.No);
                parameters.Add("@date__",
                voucherObj.Date__ >= minSqlDate ? voucherObj.Date__ : (object)DBNull.Value);
                parameters.Add("@person_account_id", voucherObj.Person_Account_Id);
                parameters.Add("@person_name", voucherObj.Person_Name);
                parameters.Add("@manual_voucher_no", voucherObj.Manual_Voucher_No);
                parameters.Add("@other_Voucher_No", voucherObj.Other_Voucher_No);
                parameters.Add("@curr_id", voucherObj.Curr_Id);
                parameters.Add("@total_amount", voucherObj.Total_Amount);
                parameters.Add("@cash_amount", voucherObj.Cash_Amount);
                parameters.Add("@check_amount", voucherObj.Check_Amount);
                parameters.Add("@delegate_id", voucherObj.Delegate_Id != 0 ? voucherObj.Delegate_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@payment_type_id", voucherObj.Payment_Type_Id != 0 ? voucherObj.Payment_Type_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@notes", voucherObj.Notes);
                parameters.Add("@printed", voucherObj.Printed);
                parameters.Add("@invoice_issued", voucherObj.Invoice_Issued);
                parameters.Add("@status", voucherObj.Status);
                parameters.Add("@deduction_percentage", voucherObj.Deduction_Percentage);
                parameters.Add("@vat", voucherObj.Vat);
                parameters.Add("@bill_tax_type", voucherObj.Bill_Tax_Type);
                parameters.Add("@maqassah_bill", voucherObj.Maqassah_Bill);
                parameters.Add("@print_with_tax", voucherObj.Print_With_Tax);
                parameters.Add("@deduction_amount", voucherObj.Deduction_Amount);
                parameters.Add("@entry_user_id", voucherObj.Entry_User_Id);
                parameters.Add("@entry_date",
          voucherObj.Entry_Date >= minSqlDate ? voucherObj.Entry_Date : (object)DBNull.Value);
                parameters.Add("@exchange_price", voucherObj.Exchange_Price);
                parameters.Add("@tagged", voucherObj.Tagged);
                parameters.Add("@customer_credit_card_no", voucherObj.Customer_Credit_Card_No);
                parameters.Add("@bank_id", voucherObj.Bank_Id);
                parameters.Add("@branch_id", voucherObj.Branch_Id);
                parameters.Add("@genarated_by_system", voucherObj.Genarated_By_System);
                parameters.Add("@account_print_name", voucherObj.Account_Print_Name);
                parameters.Add("@refrence_id", voucherObj.Refrence_Id != 0 ? voucherObj.Refrence_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@order_type_id", voucherObj.Order_Type_Id != 0 ? voucherObj.Order_Type_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@table_id", voucherObj.Table_Id != 0 ? voucherObj.Table_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@driver_id", voucherObj.Driver_Id != 0 ? voucherObj.Driver_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@start_date",
        voucherObj.Start_Date >= minSqlDate ? voucherObj.Start_Date : (object)DBNull.Value, dbType: DbType.DateTime);
                parameters.Add("@pay_time", voucherObj.Pay_Time);
                parameters.Add("@store_id", voucherObj.Store_Id);
                parameters.Add("@store_name", voucherObj.Store_Name);
                parameters.Add("@internal_consingment_id", voucherObj.Internal_Consingment_Id != 0 ? voucherObj.Internal_Consingment_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@waiter_id", voucherObj.Waiter_Id != 0 ? voucherObj.Waiter_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@due_date",
        voucherObj.Due_Date >= minSqlDate ? voucherObj.Due_Date : (object)DBNull.Value);
                parameters.Add("@batch_id", voucherObj.Batch_Id != 0 ? voucherObj.Batch_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@genarated_by_system_type_id", voucherObj.Genarated_By_System_Type_Id != 0 ? voucherObj.Genarated_By_System_Type_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@vat_reg", voucherObj.Vat_Reg);
                parameters.Add("@vat_reg_cust_supp", voucherObj.Vat_Reg_Cust_Supp);
                parameters.Add("@trans_year", voucherObj.Trans_Year);
                parameters.Add("@discount_type", voucherObj.Discount_Type);
                parameters.Add("@connected_id", voucherObj.Connected_Id);
                parameters.Add("@queue_no", voucherObj.Queue_No);
                parameters.Add("@queue_status", voucherObj.Queue_Status);
                parameters.Add("@queu_statu_time", voucherObj.Queu_Statu_Time);
                parameters.Add("@deleted_date",
        voucherObj.Deleted_Date >= minSqlDate ? voucherObj.Deleted_Date : (object)DBNull.Value, dbType: DbType.DateTime);

                await db.ExecuteAsync("sp_Vouchers_And_Bills_Insert", parameters, trans, commandType: CommandType.StoredProcedure);

                return parameters.Get<int>("@id"); // الـ Identity الجديد
            }
            catch (Exception EX)
            {
                throw;
            }

        }

        public static async Task<int> InsertJournal(IDbConnection db, IDbTransaction trans, Journal journalObj)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@voucher_id", journalObj.Voucher_Id);
                parameters.Add("@debit_credit", journalObj.Debit_Credit);
                parameters.Add("@account_serial", journalObj.Account_Serial);
                parameters.Add("@account_id", journalObj.Account_Id);
                parameters.Add("@actual_amount", journalObj.Actual_Amount);
                parameters.Add("@actual_amount_curr", journalObj.Actual_Amount_Curr);
                parameters.Add("@account_equal_amount", journalObj.Account_Equal_Amount);
                parameters.Add("@account_curr", journalObj.Account_Curr);
                parameters.Add("@base_curr_amount", journalObj.Base_Curr_Amount);
                parameters.Add("@equal_exchange_price", journalObj.Equal_Exchange_Price);
                parameters.Add("@exchange_price", journalObj.Exchange_Price);
                parameters.Add("@Notes", journalObj.Notes);
                parameters.Add("@status", journalObj.Status);
                parameters.Add("@is_tax_account", journalObj.Is_Tax_Account);
                parameters.Add("@cost_center_id", journalObj.Cost_Center_Id);
                parameters.Add("@cost_center_name", journalObj.Cost_Center_Name);

                await db.ExecuteAsync(
                    "sp_Journals_Insert",
                    parameters,
                     trans,
                    commandType: CommandType.StoredProcedure
                );

                int newId = parameters.Get<int>("@id");
                return newId;
            }
            catch (Exception EX)
            {
                throw;
            }

        }
        public static async Task<int> InsertVoucherItem(IDbConnection db, IDbTransaction trans, VouchersItemsAndServices itemObj, SaveInvoiceVoucherF.Query mainObj)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);

                parameters.Add("@voucher_id", itemObj.voucher_id);
                parameters.Add("@item_service_id", itemObj.item_service_id);
                parameters.Add("@item_unit_id", itemObj.item_unit_id == 0 ? (int?)null : itemObj.item_unit_id);
                parameters.Add("@main_unit_id", itemObj.main_unit_id == 0 ? (int?)null : itemObj.main_unit_id);
                parameters.Add("@qty_expire_date",
                itemObj.qty_expire_date >= new DateTime(2000, 1, 1)
                ? itemObj.qty_expire_date
                : (DateTime?)null);
                parameters.Add("@quantity", itemObj.quantity);
                parameters.Add("@bonus_qty", itemObj.bonus_qty);
                parameters.Add("@unit_price", itemObj.unit_price);
                parameters.Add("@notes", itemObj.notes);
                parameters.Add("@to_main_unit_qty", itemObj.to_main_unit_qty);
                parameters.Add("@base_exchange_price", itemObj.base_exchange_price);
                parameters.Add("@unit_price_with_discount_and_tax", itemObj.unit_price_with_discount_and_tax);
                parameters.Add("@cost", itemObj.cost);
                parameters.Add("@item_discount_amount", itemObj.item_discount_amount);
                parameters.Add("@item_print_name", itemObj.item_print_name);
                parameters.Add("@waiter_id", itemObj.waiter_id);
                parameters.Add("@item_vat", itemObj.item_vat);
                parameters.Add("@is_custody", itemObj.is_custody);
                parameters.Add("@is_pos_fixed", itemObj.is_pos_fixed);
                parameters.Add("@branch_id", itemObj.branch_id == 0 ? (int?)null : itemObj.branch_id);
                parameters.Add("@offer_price_id", itemObj.offer_price_id == 0 ? (int?)null : itemObj.offer_price_id);
                parameters.Add("@campaign_id", itemObj.campaign_id == 0 ? (int?)null : itemObj.campaign_id);
                parameters.Add("@camp_item_type", itemObj.camp_item_type == 0 ? (int?)null : itemObj.camp_item_type);
                parameters.Add("@item_unit_bar_code", itemObj.item_unit_bar_code);

                await db.ExecuteAsync(
                    "sp_vouchers_items_and_services_Insert",
                    parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure
                );

                bool updatePrice = await UpdateItemInfo(db, trans, itemObj, mainObj);

                return parameters.Get<int>("@id");
            }
            catch (Exception EX)
            {
                throw;
            }
        }

        // فاتورة مشتريات لتعديل شعر الشراء للصنف
        public static async Task<bool> UpdateItemInfo(IDbConnection db, IDbTransaction trans, VouchersItemsAndServices itemObj,
                                                      SaveInvoiceVoucherF.Query mainObj)
        {
            try
            {
                if (mainObj.voucherObj.Type != "H") return true;
                double _price = 0;
                string sql = @"
                        SELECT *
                        FROM dbo.Items_and_services
                        WHERE id = @ItemId ";

                string typeId = "1"; DateTime lpPriceDate = DateTime.Now;
                var resultSelect = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { ItemId = itemObj.item_service_id }, trans);
                if (resultSelect != null)
                {
                    typeId = resultSelect.type_id;
                    lpPriceDate = resultSelect.lp_price_date ?? DateTime.MinValue;
                }

                if (typeId == "1")//صنف
                {
                    if (lpPriceDate <= mainObj.voucherObj.Date__ || (lpPriceDate.ToShortDateString() == mainObj.voucherObj.Date__.ToShortDateString()))// عمل تعديل اذا كان تاريخ ادخال الفاتورة اكبر او يساوي  تاريخ اخر سعر شراء 
                    {
                        double _vatVal = (mainObj.voucherObj.Vat / 100) + 1;


                        _price = itemObj.unit_price_with_discount_and_tax / _vatVal;

                        double itemDiscountWithOutVat = itemObj.item_discount_amount;
                        double voucherAmountDiscountWithOutVat = mainObj.voucherObj.Deduction_Amount;
                        double voucherTotalAmountWithOutVat = mainObj.voucherObj.Total_Amount;
                        if (mainObj.voucherObj.Total_Amount > 0 && _vatVal > 0)
                            voucherTotalAmountWithOutVat = mainObj.voucherObj.Total_Amount / _vatVal;

                        double priceWithOutVat = _price;
                        int Lp_Price_Curr = mainObj.voucherObj.Curr_Id;
                        _price = _price / itemObj.to_main_unit_qty;
                        /////////////////////////////////////////////////////////////

                        ////////////////////////////////////////////////////////////
                        double Last_Purchase_Price = _price;
                        double Last_Pay_Price_Withoutdiscount = _price;

                        DateTime Lp_Price_Date = mainObj.voucherObj.Date__; //DataLayer.Universal.GetDateTimeNowFromServer(con, user_id, Trans);

                        _price = _price * mainObj.voucherObj.Exchange_Price;
                        double Last_Purchase_Price_Maint_Unit = _price;
                        double Last_Pay_Price_Main_Withoutdiscount = _price;
                        int Lp_Price_Curr_Main_Unit = 1;
                        DateTime Lp_Price_Date_Main_Unit = mainObj.voucherObj.Date__; // DataLayer.Universal.GetDateTimeNowFromServer(con, user_id, Trans);

                        // 22/09/2020 زيد+امجد الخفش/ ابو غالب/ اضافة حقل اخر في ملف الصنف (سعر الشراء مطروح الخصم والبونص)
                        //مبلغ شراء االصنف = مبلغ شراء الصنف - الخصم على مستوى الصنف (اي المبلغ الصافي)
                        double result = 0;
                        if (mainObj.voucherObj.Total_Amount > 0)
                            result = (((priceWithOutVat * itemObj.quantity) - itemDiscountWithOutVat) / (voucherTotalAmountWithOutVat + voucherAmountDiscountWithOutVat)) * voucherAmountDiscountWithOutVat;
                        //سعر شراء وحدة الصنف المخصوم = ((سعر شراء وحدة الصنف * كمية الشراء) - مبلغ خصم الصنف على كامل الكمية - حصة خصم الصنف من خصم الفاتورة ) / (الكمية + البونص)
                        double unitPriceWithDiscountAndTax = 0;
                        if ((itemObj.quantity + itemObj.bonus_qty) > 0)
                            unitPriceWithDiscountAndTax = ((priceWithOutVat * itemObj.quantity)
                                                              - itemDiscountWithOutVat - result) / (itemObj.quantity + itemObj.bonus_qty);
                        if (unitPriceWithDiscountAndTax > 0)
                        {
                            double resultFinal = (unitPriceWithDiscountAndTax / itemObj.to_main_unit_qty);
                            Last_Pay_Price_Withoutdiscount = resultFinal;
                            Last_Pay_Price_Main_Withoutdiscount = resultFinal * mainObj.voucherObj.Exchange_Price;
                        }

                    string updateQuery = @"
                    UPDATE Items_and_services
                    SET 
                        Last_Purchase_Price = @Last_Purchase_Price,
                        Last_Pay_Price_Withoutdiscount=@Last_Pay_Price_Withoutdiscount,
                        Lp_Price_Date_Main_Unit=@Lp_Price_Date_Main_Unit,
                        Last_Purchase_Price_Maint_Unit=@Last_Purchase_Price_Maint_Unit,
                        Lp_Price_Date=@Lp_Price_Date,
                        Last_Pay_Price_Main_Withoutdiscount=@Last_Pay_Price_Main_Withoutdiscount,
                        Lp_Price_Curr_Main_Unit=@Lp_Price_Curr_Main_Unit
                        WHERE id = @id ";
                        int rows = await db.ExecuteAsync(updateQuery, new
                        {
                            id = itemObj.item_service_id,
                            Last_Purchase_Price,
                            Last_Pay_Price_Withoutdiscount,
                            Lp_Price_Date_Main_Unit,
                            Last_Purchase_Price_Maint_Unit,
                            Lp_Price_Date,
                            Last_Pay_Price_Main_Withoutdiscount,
                            Lp_Price_Curr_Main_Unit
                        }, trans);
                    }
                }
                return true;
            }
            catch (Exception EX)
            {
                throw;
            }
        }


        private static async Task<bool> UpdateItemsWhenDeleteVoucher(IDbConnection db, IDbTransaction trans,
                                                                    dynamic dt_TopOne, int itemId)
        {
            try
            {
                // اخذ اخر فاتورة مشتريات
                // اذا لم يوجد سوى سندات ادخال يتم اخذ اول سند ادخال
                double _vat = 0; double _price = 0;
                _vat = dt_TopOne.First().vat;
                double _vatVal = (_vat / 100) + 1;
                double unitPriceWithOutTax = dt_TopOne.First().unit_price_with_discount_and_tax;
                _price = unitPriceWithOutTax;
                if (unitPriceWithOutTax > 0 && _vatVal > 0)
                    _price = unitPriceWithOutTax / _vatVal;
                double Lp_Price_Curr = dt_TopOne.First().curr_id;
                double toMainUnitQty = dt_TopOne.First().to_main_unit_qty;
                _price = _price / toMainUnitQty;
                double Last_Purchase_Price = _price;
                DateTime Lp_Price_Date = dt_TopOne.First().date;
                double exchangPrice = dt_TopOne.First().exchange_price;
                _price = _price * exchangPrice;
                double Last_Purchase_Price_Maint_Unit = _price;
                DateTime Lp_Price_Date_Main_Unit = dt_TopOne.First().date;
                #region 02/11/2020 update Last_Pay_Price_Withoutdiscount
                ////////////////////////////////////////////////////////// 02/11/2020
                double quantity = 1; double bouns = 0; double voucherDeductionAmountWithOutVat = 0;
                double itemDiscountWithOutVat = dt_TopOne.First().item_discount_amount;

                double result = 0;

                quantity = dt_TopOne.First().quantity;
                bouns = dt_TopOne.First().bonus_qty;
                voucherDeductionAmountWithOutVat = dt_TopOne.First().deduction_amount;

                double voucherTotalAmountWithOutVat = dt_TopOne.First().total_amount;
                if (voucherTotalAmountWithOutVat > 0 && _vatVal > 0)
                    voucherTotalAmountWithOutVat = voucherTotalAmountWithOutVat / _vatVal;
                if (voucherTotalAmountWithOutVat > 0)
                    result = (((_price * quantity) - itemDiscountWithOutVat) / (voucherTotalAmountWithOutVat + voucherDeductionAmountWithOutVat)) * voucherDeductionAmountWithOutVat;
                //سعر شراء وحدة الصنف المخصوم = ((سعر شراء وحدة الصنف * كمية الشراء) - مبلغ خصم الصنف على كامل الكمية - حصة خصم الصنف من خصم الفاتورة ) / (الكمية + البونص)
                double unitPriceWithDiscountAndTax = 0;
                if ((quantity + bouns) > 0)
                    unitPriceWithDiscountAndTax = ((_price * quantity)
                                                      - itemDiscountWithOutVat - result) / (quantity + bouns);
                double Last_Pay_Price_Withoutdiscount = 0, Last_Pay_Price_Main_Withoutdiscount = 0;
                if (unitPriceWithDiscountAndTax > 0)
                {
                    double resultFinal = (unitPriceWithDiscountAndTax / toMainUnitQty);
                    Last_Pay_Price_Withoutdiscount = resultFinal;
                    Last_Pay_Price_Main_Withoutdiscount = resultFinal * exchangPrice;
                }
                int Lp_Price_Curr_Main_Unit = 1;
                ////////////////////////////////////////////////////////// 02/11/2020
                #endregion 02/11/2020 update Last_Pay_Price_Withoutdiscount
                string updateQuery = @"
                    UPDATE Items_and_services
                    SET 
                        Last_Purchase_Price = @Last_Purchase_Price,
                        Last_Pay_Price_Withoutdiscount=@Last_Pay_Price_Withoutdiscount,
                        Lp_Price_Date_Main_Unit=@Lp_Price_Date_Main_Unit,
                        Last_Purchase_Price_Maint_Unit=@Last_Purchase_Price_Maint_Unit,
                        Lp_Price_Date=@Lp_Price_Date,
                        Last_Pay_Price_Main_Withoutdiscount=@Last_Pay_Price_Main_Withoutdiscount,
                        Lp_Price_Curr_Main_Unit=@Lp_Price_Curr_Main_Unit
                        WHERE id = @id ";
                int rows = await db.ExecuteAsync(updateQuery, new
                {
                    id = itemId,
                    Last_Purchase_Price,
                    Last_Pay_Price_Withoutdiscount,
                    Lp_Price_Date_Main_Unit,
                    Last_Purchase_Price_Maint_Unit,
                    Lp_Price_Date,
                    Last_Pay_Price_Main_Withoutdiscount,
                    Lp_Price_Curr_Main_Unit
                }, trans);
                return true;
            }
            catch
            {
                throw;
            }
        }

    
    }

}
