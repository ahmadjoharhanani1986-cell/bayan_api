using System.Collections;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using SHLAPI.Features.VoucherQabdSarf;
using SHLAPI.Models.Settings;
using static SHLAPI.Models.InvoiceVoucher.InvoiceVoucherGetData_M;

namespace SHLAPI.Models.VoucherQabdSarf
{
    public class VoucherQabdSarf_M
    {
        public IEnumerable<Currency_M> Currencies { get; set; }
        public IEnumerable<PaymentType_M> PaymentTypes { get; set; }
        public IEnumerable<UserTreasury_M> UserTreasury { get; set; }
        public string MaxVoucherNo { get; set; }
        public int CurrencyFractionCount { get; set; }
        public IEnumerable<Bank_M> Banks { get; set; }
        public IEnumerable<Branch_M> Branches { get; set; }
        public IEnumerable<Delegate_M> delegates { get; set; }
        public class Currency_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

        public class Bank_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Branch_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Delegate_M
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static async Task<VoucherQabdSarf_M> LoadQabdSarfScreen(
            IDbConnection db,
            IDbTransaction trans,
            int userId,
            int currencyId,
            string type)
        {
            try
            {
               // 1) Currencies
var currencies = await db.QueryAsync<Currency_M>(
    "SELECT id, name FROM Currency ORDER BY id ASC",
    transaction: trans);

// 2) Payment Types
var paymentTypes = await db.QueryAsync<PaymentType_M>(
    "SELECT id, name FROM Payment_Types ORDER BY name ASC",
    transaction: trans);

// 3) User Treasury
var userTreasury = await db.QueryAsync<UserTreasury_M>(
    @"SELECT 
        id, 
        user_id AS UserId, 
        currency_id AS CurrencyId, 
        cash_account_id, 
        check_account_id, 
        notes 
      FROM Users_Treasury_Rights 
      WHERE user_id=@userId AND currency_id=@currencyId",
    new { userId, currencyId },
    transaction: trans);

// 4) Max Voucher Number (MySQL version)
string sql = type == "V"
    ? "SELECT IFNULL(MAX(no),0) FROM Internal_Consingment"
    : "SELECT IFNULL(MAX(no),0) FROM Vouchers_And_Bills WHERE type=@type";

var maxVoucherNo = await db.ExecuteScalarAsync<string>(
    sql,
    new { type },
    transaction: trans);
    if(maxVoucherNo=="0")maxVoucherNo = type + "00000000";

// 5) Currency Fraction Count
var units = await db.ExecuteScalarAsync<string>(
    "SELECT units FROM Currency WHERE id=@id",
    new { id = currencyId },
    transaction: trans);

int fractionCount = !string.IsNullOrEmpty(units) ? units.Length - 1 : 0;

// 6) Banks
var banks = await db.QueryAsync<Bank_M>(
    "SELECT id, name FROM Banks ORDER BY no",
    transaction: trans);

// 7) Branches
var branches = await db.QueryAsync<Branch_M>(
    "SELECT id, name FROM Bank_Branches",
    transaction: trans);

// 8) Delegates
var delegats = await db.QueryAsync<Delegate_M>(
    "SELECT id, name FROM Delegates",
    transaction: trans);

// Return object
return new VoucherQabdSarf_M
{
    Currencies = currencies,
    PaymentTypes = paymentTypes,
    UserTreasury = userTreasury,
    MaxVoucherNo = maxVoucherNo,
    CurrencyFractionCount = fractionCount,
    Banks = banks,
    Branches = branches,
    delegates = delegats
};


            }
            catch (Exception ex)
            {
                // log error if needed
                return null;
            }
        }
    }

    public class VoucherQabdSarfGetData_M
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
        public static async Task<VoucherQabdSarfGetData_M> GetData(IDbConnection db, IDbTransaction trans, bool _getMaxNoFromService, int voucherId, int userId,
                                                                   string _type, DateTime _date, string voucherNo, string _viewName)
        {

            var resultObj = new VoucherQabdSarfGetData_M();
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


        public static async Task<dynamic> CheckDateOfVoucher(IDbConnection db, string type, IDbTransaction trans)
        {
            try
            {
                string where = string.Format(" 1=1 and type='{0}' and status='0' order by date desc", type);
                var result = await db.QueryFirstOrDefaultAsync<dynamic>(
                                "Vouchers_And_Bills_Check_Date_of_Voucher_GetAllByWhere",
                                new { Str1 = where },
                                commandType: CommandType.StoredProcedure,
                                    transaction: trans
                                );
                var list = result;
                return list;
            }
            catch (Exception EX)
            {
                throw;
            }
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

        public static string ConvertToSeverDateTimeFormateString(DateTime date)
        {
            // StreamWriter streamWriter = new StreamWriter("C:\\log\\log.txt");
            // streamWriter.WriteLine("johar="+date + "");
            // streamWriter.Close();
            // إذا بدك التاريخ فقط
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
            VoucherQabdSarfGetData_M voucherObj = await GetData(db, trans, false, voucherId, userId, _type, _date, voucherNo, _viewName);

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
                
                if (_type == "H") // فاتورة مشتريات
                {
                    // تعديل اخر سعر شراء للاصناف المراد حذف فاتورتها حسب اخر فاتورة مشتريات او اخر فاتوره سند ادخال
                    string sql = @"
                        SELECT *
                        FROM dbo.vouchers_items_and_services
                        WHERE voucher_id = @voucherId ";

                    var voucherAndServiceObj = await db.QueryAsync<dynamic>(sql, new { voucherId }, trans);

                    if (voucherAndServiceObj == null)
                    {
                        returnResult.result = false;
                        returnResult.msg = "error";
                        return returnResult;
                    }
                    foreach (dynamic voucherItem in voucherAndServiceObj)
                    {

                        sql = @"
                        SELECT *
                        FROM dbo.Items_and_services
                        WHERE id = @ItemId ";

                        string typeId = "1"; DateTime lpPriceDate = DateTime.Now;
                        var resultSelect = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { ItemId = voucherItem.item_service_id }, trans);
                        if (resultSelect != null)
                        {
                            typeId = resultSelect.type_id;
                            lpPriceDate = resultSelect.lp_price_date ?? DateTime.MinValue;
                        }


                        //BusinessLayer.vouchers_items_and_services _voucherItem = new BusinessLayer.vouchers_items_and_services((int)dt_voucherItems.Rows[i]["id"], con, false, _objDeleteTransaction.userId, Trans);

                        bool isUpdatedItems = true;
                        if (typeId == "1")//صنف
                        {
                            var dt_TopOne = await GetByDynamicSearchSp(db, "ItemServiceMaxPayPrice_v", " where item_service_id=" + voucherItem.item_service_id + " and voucher_id <>" + voucherId + " ORDER BY date DESC, voucher_id Desc", "Top(1) *", trans);
                            if (dt_TopOne != null && dt_TopOne.Count > 0)
                            {
                                isUpdatedItems = await UpdateItemsWhenDeleteVoucher(db, trans, dt_TopOne, voucherItem.item_service_id);
                            }
                            else
                            {
                                var dt_TopOneA = await GetByDynamicSearchSp(db, "ItemServiceMax_A_Price_v", " where item_service_id=" + voucherItem.item_service_id + " and voucher_id <>" + voucherId + " ORDER BY date asc, voucher_id asc", "Top(1) *", trans);
                                if (dt_TopOneA != null && dt_TopOneA.Count > 0)
                                {
                                    isUpdatedItems = await UpdateItemsWhenDeleteVoucher(db, trans, dt_TopOneA, voucherItem.item_service_id);
                                }
                                else// لا يوجد قبله اي فواتير مشتريات ولا يوجد سند ادخال
                                {
                                    double Last_Purchase_Price = 0;
                                    DateTime? Lp_Price_Date = null; // ✅ Nullable
                                    double Last_Purchase_Price_Maint_Unit = 0;
                                    int Lp_Price_Curr_Main_Unit = 1;
                                    DateTime? Lp_Price_Date_Main_Unit = null; // ✅ Nullable
                                    double Last_Pay_Price_Withoutdiscount = 0;
                                    double Last_Pay_Price_Main_Withoutdiscount = 0;
                                    updateQuery = @"
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
                                    rows = await db.ExecuteAsync(updateQuery, new
                                    {
                                        id = voucherItem.item_service_id,
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

                        }
                    }
                }
           
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
            public List<Check> Checks { get; set; }
            public List<CheckTrans> ChecksTrans { get; set; }
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
        public class Check
        {
            public string in_out { get; set; }
            public string no { get; set; }
            public int bank_branch_id { get; set; }
            public double amount { get; set; }
            public int curr_id { get; set; }
            public DateTime first_due_date { get; set; }
            public DateTime due_date { get; set; }
            public int customer_account_id { get; set; }
            public string customer_bank_account { get; set; }
            public string notes { get; set; }
            public string status { get; set; }
            public double exchange_price { get; set; }
            public double voucher_exchange_price { get; set; }
            public int voucher_id { get; set; }
            public int delegate_id { get; set; }
            public string coa_bank_account { get; set; }
            public bool prev_year { get; set; }
            public int prev_year_last_state { get; set; }
            public int state_id { get; set; }
            public int front_img_id { get; set; }
            public int back_img_id { get; set; }
            public string front_img_path { get; set; }
            public string back_img_path { get; set; }
        }
        public class CheckTrans
        {
            public int operation_Id { get; set; }
            public DateTime voucher_Date { get; set; }
            public int operation_Account { get; set; }
            public double exchange_Price { get; set; }
            public int check_Current_State { get; set; }
            public string status { get; set; }
            public int physical_Account_Id { get; set; }
            public DateTime actual_Operation_Date { get; set; }
            public string voucher_no { get; set; }
            public int check_id { get; set; }
            public DateTime deletion_date { get; set; }
            public DateTime check_current_due_date { get; set; }
            public string feesheh_no { get; set; }
        }

        public static async Task<VoucherResult> SaveVouchersQabdAndSarf(IDbConnection db, IDbTransaction trans, SaveQabdSarfVoucherF.Query obj)
        {
            VoucherResult returnObj = new VoucherResult();
            returnObj.result = true;
            try
            {
                if (obj.checkList != null)
                {
                    foreach (Check checkObj in obj.checkList)
                    {

                        bool checkDuplicated = await ChecksIfChecksNotDublicate(db, trans, checkObj.no, obj.user_id, checkObj.customer_bank_account);
                        if (checkDuplicated)
                        {
                            returnObj.result = false;
                            returnObj.msg = "duplicatedCheckNo";
                            returnObj.value = checkObj.no;
                            return returnObj;
                        }

                    }
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                QabdQuickValidationClass _res = await QabdQuickValidation(db, trans, obj.voucherObj.Curr_Id, obj.user_id,
                                          obj.voucherObj.Person_Account_Id, obj.voucherObj.Curr_Id,
                                          obj.voucherObj.Date__,
                                         false, obj.voucherObj.Type);


                if (_res.allDataOnSelectedCmbCurrencyChanged != AllDataOnSelectedCmbCurrencyChanged.Succssfull)
                {
                    returnObj.result = false;
                    returnObj.msg = _res.allDataOnSelectedCmbCurrencyChanged + "";
                    return returnObj;
                }

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

                List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);
                string _voucherMaxNo = await GetMaxVoucherNOAsync(obj.voucherObj.Type, db, obj.user_id, obj.voucherObj.Date__, settingList, trans);
                string _maxNoPlusOne = obj.voucherObj.Type + Inc_Code(_voucherMaxNo, obj.voucherObj.Date__, settingList);
                ///////////////////////////////////////////////////////////////////////////////////////////

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
                obj.voucherObj.No = _maxNoPlusOne;
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
                    if (journalObj.Account_Serial == -1) _childAccountEqualAmount = journalObj.Account_Equal_Amount;
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


                ArrayList listCheckId = new ArrayList();
                int _checkCount = 0;
                if (obj.checkList != null)
                {
                    foreach (Check checks in obj.checkList)
                    {
                        checks.voucher_id = obj.voucherObj.Id;
                        ++_checkCount;
                        int checkSave = await InsertCheck(db, trans, checks);
                        if (checkSave <= 0)
                        {
                            returnObj.result = false;
                            returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.Error + "";
                            return returnObj;
                        }
                        listCheckId.Add(checkSave);
                    }
                }

                if (obj.checkTransList != null)
                {
                    int checkCounter = 0;
                    foreach (CheckTrans checksTrans in obj.checkTransList)
                    {
                        checksTrans.voucher_no = obj.voucherObj.No;
                        checksTrans.check_id = (int)listCheckId[checkCounter];
                        checksTrans.status = "0";
                        checksTrans.check_Current_State = -1;
                        checksTrans.actual_Operation_Date = DateTime.Now;
                        int checkTransSave = await InsertCheckTrans(db, trans, checksTrans);
                        if (checkTransSave <= 0)
                        {
                            returnObj.result = false;
                            returnObj.msg = AllDataOnSelectedCmbCurrencyChanged.Error + "";
                            return returnObj;
                        }
                        ++checkCounter;
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
            CostCenterStopped
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
     public static async Task<GetExchangeCurrencyPriceByDateClass> GetExchangeCurrencyPriceByDate(
    int currencyId,
    DateTime date,
    IDbConnection db,
    bool throwException,
    IDbTransaction trans = null)
{
    var returnObj = new GetExchangeCurrencyPriceByDateClass
    {
        ExchangePrice = 0,
        result = false
    };

    try
    {
        // Normalize date (same logic as SQL Server)
        date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

        string sql = @"
        SELECT exchange_price
        FROM Currency_prices
        WHERE currency_id = @currencyId
          AND date = @date
        LIMIT 1;
        ";

        var price = await db.QueryFirstOrDefaultAsync<double?>(
            sql,
            new { currencyId, date },
            transaction: trans
        );

        if (price.HasValue)
        {
            returnObj.ExchangePrice = price.Value;
            returnObj.result = true;
        }

        return returnObj;
    }
    catch
    {
        if (throwException)
            throw;

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

        public static async Task<int> InsertCheck(IDbConnection db, IDbTransaction trans, Check check)
        {

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@in_out", check.in_out);
                parameters.Add("@voucher_id", check.voucher_id);
                parameters.Add("@no", check.no);
                parameters.Add("@bank_branch_id", check.bank_branch_id);
                parameters.Add("@amount", check.amount);
                parameters.Add("@curr_id", check.curr_id);
                parameters.Add("@first_due_date", check.first_due_date);
                parameters.Add("@due_date", check.due_date);
                parameters.Add("@delegate_id", check.delegate_id != 0 ? check.delegate_id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@customer_bank_account", check.customer_bank_account);
                parameters.Add("@customer_account_id", check.customer_account_id);
                parameters.Add("@coa_bank_account", check.coa_bank_account);
                parameters.Add("@prev_year", check.prev_year);
                parameters.Add("@prev_year_last_state", -1);
                parameters.Add("@notes", check.notes);
                parameters.Add("@state_id", check.state_id);
                parameters.Add("@status", check.status);
                parameters.Add("@exchange_price", check.exchange_price);
                parameters.Add("@voucher_exchange_price", check.voucher_exchange_price);
                parameters.Add("@front_img_id", check.front_img_id != 0 ? check.front_img_id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@back_img_id", check.back_img_id != 0 ? check.back_img_id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@front_img_path", check.front_img_path);
                parameters.Add("@back_img_path", check.back_img_path);

                await db.ExecuteAsync(
                    "sp_Checks_Insert",
                    parameters,
                    trans,
                    commandType: CommandType.StoredProcedure
                );

                return parameters.Get<int>("@id"); // يرجع الـ id الجديد
            }
            catch (Exception EX)
            {
                throw;
            }
        }
        public static async Task<int> InsertCheckTrans(IDbConnection db, IDbTransaction trans, CheckTrans checkTrans)
        {
            try
            {
                DateTime minSqlDate = new DateTime(1753, 1, 1);
                var parameters = new DynamicParameters();
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@operation_id", checkTrans.operation_Id);
                parameters.Add("@voucher_no", checkTrans.voucher_no);
                parameters.Add("@voucher_date", checkTrans.voucher_Date);
                parameters.Add("@operation_account", checkTrans.operation_Account);
                parameters.Add("@exchange_price", checkTrans.exchange_Price);
                parameters.Add("@check_id", checkTrans.check_id);
                parameters.Add("@status", checkTrans.status);
                parameters.Add("@check_current_state", checkTrans.check_Current_State);
                parameters.Add("@deletion_date", checkTrans.deletion_date >= minSqlDate ?
                 checkTrans.deletion_date : (object)DBNull.Value, dbType: DbType.DateTime);
                parameters.Add("@check_current_due_date", checkTrans.check_current_due_date >= minSqlDate ?
                 checkTrans.check_current_due_date : (object)DBNull.Value, dbType: DbType.DateTime);
                parameters.Add("@feesheh_no", checkTrans.feesheh_no);
                parameters.Add("@physical_account_id", checkTrans.physical_Account_Id != 0 ? checkTrans.physical_Account_Id : DBNull.Value, dbType: DbType.Int32);
                parameters.Add("@actual_operation_date", checkTrans.actual_Operation_Date >= minSqlDate ?
                 checkTrans.actual_Operation_Date : (object)DBNull.Value, dbType: DbType.DateTime);

                await db.ExecuteAsync(
                    "sp_Check_Trans_Insert",
                    parameters,
                    trans,
                    commandType: CommandType.StoredProcedure
                );

                return parameters.Get<int>("@id"); // يرجع ID العملية
            }
            catch (Exception EX)
            {
                throw;
            }

        }


        // private void OpenPhysicalAccountSearch()
        // {
        //     try
        //     {
        //         string _where = "";
        //         if (cmbCurrency.SelectedValue != null)
        //             _where = " curr_id='" + cmbCurrency.SelectedValue + "'"; // //MLHIDE
        //         frmSearch _search = new frmSearch("bank_Physical_Account_view", ml.ml_string(1404, "البحث عن الحسابات البنكية"), _where, true, true, true, false, false, false); // //MLHIDE
        //         _search.ShowDialog(this);
        //         if (_search._primaryKey != null)
        //         {
        //             cmbChecksBoxes.ISCO_MemberValue = _search._primaryKey.ToString();
        //             bankPhysicalAccountId = (int)_search._primaryKey;
        //         }
        //         if (cmbChecksBoxes.ISCO_MemberValue != null && cmbChecksBoxes.ISCO_MemberValue.ToString() != "")
        //         {
        //             DataRow _dr = grdChecks.GetDataRow(0);
        //             if (_dr != null && _dr["TxtCheckAmount"] == null || _dr["TxtCheckAmount"] + "" == "0" // //MLHIDE
        //                   || _dr["TxtCheckAmount"] + "" == "") //     //MLHIDE
        //             {
        //                 grdChecks.SetRowCellValue(0, grdChecks.Columns["TxtCheckAmount"], decimal.Parse(txtChkAmount.Text)); //MLHIDE
        //             }
        //             grdChecks.FocusedColumn = grdChecks.Columns["TxtCheckNo"]; //MLHIDE
        //             grdChecks.FocusedRowHandle = 0;
        //             grdChecks.Focus();
        //         }
        //     }
        //     catch (Exception EX)
        //     {
        //         ChangeNotification(MsgType.Error, EX.ToString());
        //         Utility.InsertException(EX, con);
        //     }
        // }

                private static async Task<bool> UpdateItemsWhenDeleteVoucher(IDbConnection db, IDbTransaction trans,
                                                                    dynamic dt_TopOne, int itemId)
        {
            try
            {
                // اخذ اخر فاتورة مشتريات
                // اذا لم يوجد سوى سندات ادخال يتم اخذ اول سند ادخال
                double _vat = 0; double _price = 0;
                _vat = dt_TopOne[0].vat;
                double _vatVal = (_vat / 100) + 1;
                double unitPriceWithOutTax = dt_TopOne[0].unit_price_with_discount_and_tax;
                _price = unitPriceWithOutTax;
                if (unitPriceWithOutTax > 0 && _vatVal > 0)
                    _price = unitPriceWithOutTax / _vatVal;
                double Lp_Price_Curr = dt_TopOne[0].curr_id;
                double toMainUnitQty = dt_TopOne[0].to_main_unit_qty;
                _price = _price / toMainUnitQty;
                double Last_Purchase_Price = _price;
                DateTime Lp_Price_Date = dt_TopOne[0].date;
                double exchangPrice = dt_TopOne[0].exchange_price;
                _price = _price * exchangPrice;
                double Last_Purchase_Price_Maint_Unit = _price;
                DateTime Lp_Price_Date_Main_Unit = dt_TopOne[0].date;
                #region 02/11/2020 update Last_Pay_Price_Withoutdiscount
                ////////////////////////////////////////////////////////// 02/11/2020
                double quantity = 1; double bouns = 0; double voucherDeductionAmountWithOutVat = 0;
                double itemDiscountWithOutVat = dt_TopOne[0].item_discount_amount;

                double result = 0;

                quantity = dt_TopOne[0].quantity;
                bouns = dt_TopOne[0].bonus_qty;
                voucherDeductionAmountWithOutVat = dt_TopOne[0].deduction_amount;

                double voucherTotalAmountWithOutVat = dt_TopOne[0].total_amount;
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
