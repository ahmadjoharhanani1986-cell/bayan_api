using System.Data;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Utilities;
using System.Text;
using SHLAPI.Features.StatmentOfAccountsRpt;

namespace SHLAPI.Models.StatmentOfAccountsRpt
{
    public class StatmentOfAccountsRpt_M
    {
        public int ser { get; set; }
        public int jID { get; set; }
        public string ChecksDetails { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public float Qnt { get; set; }
        public float Bonus { get; set; }
        public float Price { get; set; }
        public float item_discount_amount { get; set; }
        public float Amount { get; set; }
        public DateTime Voucher_Date { get; set; }
        public string Voucher_No { get; set; }
        public int Voucher_id { get; set; }
        public string Voucher_type_char { get; set; }
        public string Voucher_Type { get; set; }
        public float Rate { get; set; }
        public string Curr { get; set; }
        public float Debit { get; set; }
        public float Credit { get; set; }
        public float Balance { get; set; }
        public string Notes { get; set; }
        public string refrence_id { get; set; }
        public string costCenterName { get; set; }
        public int costCenterId { get; set; }
        public string delegateName { get; set; }
        public string bookNo { get; set; }
        public int orderNo { get; set; }
        public float Unpayed_Checks { get; set; }
        public float Returned_Checks { get; set; }

        public static async Task<IEnumerable<StatmentOfAccountsRpt_M>> GetData(IDbConnection db, IDbTransaction trans, StatmentOfAccountsRptF.Query req)
        {
            try
            {
                req.prevBalanceCaption = "رصيد سابق";

                // --@Int321=> @langID
                // --@Int322=> @isBaseCurr
                // --@Int323=> @accID
                // --@DateTime1=> @fromDate
                // --@DateTime2=> @toDate
                // --@Str1=> @prevBalCaption
                // --@Int324=> with invoice details=1 or not=0
                // --@Int325=> from right=1 , from left=2
                // --@Int326=> with checks details=1 or not=0
                // --@Int327=> Curr id
                // --@Int328=> Trans By Curr
                // --@Int329=> استثناء قيود اقفال الدخل والضريبة 1=استثناء، 0=عدم استثناء
                // --@Int3210=> اظهار رقم السند اليدوي في عمود الملاحظة
                // --@Int32100=> احتساب رصيد الشيكات
                // --@Str2=> client pc date format
                // --@Int3211 => PrevDebitCredit
                // --@Int3212 => اظهار حالة الشيك بالطباعة
                var fromDate = req.from_date.Date; // يبدأ من 00:00:00
                var toDate = req.to_date.Date.AddDays(1).AddSeconds(-1); // يضيف يوم ويحذف ثانية = 23:59:59
                var param = new
                {
                    Int321 = req.lang_id,
                    Int322 = req.base_curr,
                    Int323 = req.account_id,
                    DateTime1 = fromDate,
                    DateTime2 = toDate,
                    Str1 = req.prevBalanceCaption ?? "",
                    Int324 = req.withInvoicesDetails,
                    Int325 = req.fromRight,
                    Int326 = req.withChecksDetails,
                    Int327 = req.currency_id,
                    Int328 = req.transByCurr,
                    Int329 = req.hideEqfal,
                    Int3210 = req.showManualNo,
                    Str2 = req.checksDateFormat,
                    Int3211 = req.prevDebitCredit,
                    Int3212 = req.printChecksDetails
                };
                var res = await db.QueryAsync<StatmentOfAccountsRpt_M>(
                    "StatementOfAccount_Report_Classic_sp",
                     param,
                     transaction: trans,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 300 // 5 minutes
                );
                return res;
            }
            catch (Exception EX)
            {

            }
            return null;
        }


        public static async Task<IEnumerable<StatmentOfAccountsRpt_M>> GetCheqBalance(IDbConnection db, IDbTransaction trans, StatmentOfAccountsRptF.Query req)
        {


            // --@Int321 int=> CurrID ,@Int322=> isBaseCurr ,@Int323=> get Checks Details, @Int324=> delegate (-1 delegate not in where)
            // --@DateTime1 datetime=> ToDate
            // --@Str1 nvarchar(20)=> fromAccount ,@Str2 nvarchar(20)=> toAccount
            // --@Float1=> fromValue ,@Float2=> toValue
            // --@Str3=> type Filter
            // --@Int325=> حسابات العملة المختارة فقط
            // --@Int326=> تصنيف الزبون
            // --@Int327=> قطاع العمل
            // --@Int328=> المنطقة
            // --@Int329=> التخصص
            // --@Int3210=> المناطق الفرعية
            // --@Int3211=> الارصدة التي تحركت من تاريخ الى تاريخ ,
            // --@DateTime2=> تاريخ بداية تحرك الارصدة 
            // --@DateTime3=> تاريخ نهاية تحرك الارصدة
            req.accountPrefix = req.from_code != null && req.from_code.ToCharArray().Length > 0 ? req.from_code.ToCharArray()[0] + "" : "";
            var param = new
            {
                Int321 = req.currency_id,
                DateTime1 = new DateTime(2050, 1, 1),
                Str1 = req.from_code,
                Str2 = req.to_code,
                Float1 = -999999999,
                Float2 = 999999999,
                Int322 = req.base_curr,
                Int323 = req.withChecksDetails,
                Str3 = req.accountPrefix,
                Int324 = -1,
                Int325 = 0,
                Int326 = -1,
                Int327 = -1,
                Int328 = -1,
                Int329 = -1,
                Int3210 = -1,
                Int3211 = 0,
                DateTime2 = DateTime.Now,
                DateTime3 = DateTime.Now

            };
            var res = await db.QueryAsync<StatmentOfAccountsRpt_M>(
                "BalancesByDateCES_Report_sp",
                 param,
                 transaction: trans,
                commandType: CommandType.StoredProcedure
            );
            // txtUnpayedChecks.Text = ""; txtReturnedChecks.Text = ""; txtBalance.Text = "";
            // if (dt_CESBalances != null && dt_CESBalances.Rows.Count > 0)
            // {
            //     #region الرصيد بعد اضافة رصيد الشيكات
            //     double finalBalance = 0, unpayedChecks = 0, returnedChecks = 0;
            //     Balance = (double)grdTransactions.Columns["Balance"].SummaryItem.SummaryValue; //MLHIDE
            //     double.TryParse(Balance.ToString(), out finalBalance);
            //     bool isMinus = false;
            //     if (finalBalance < 0)
            //     {
            //         // finalBalance = finalBalance * -1;
            //         isMinus = true;
            //     }
            //     double.TryParse(dt_CESBalances.Rows[0]["Unpayed_Checks"].ToString(), out unpayedChecks); //MLHIDE
            //     double.TryParse(dt_CESBalances.Rows[0]["Returned_Checks"].ToString(), out returnedChecks); //MLHIDE
            //     /*if (unpayedChecks < 0) unpayedChecks *= -1;
            //     if (returnedChecks < 0) returnedChecks *= -1;*/

            //     txtUnpayedChecks.Text = String.Format("{0:N" + Utility.GetFraction + "}", unpayedChecks);//unpayedChecks.ToString(); //MLHIDE
            //     txtReturnedChecks.Text = String.Format("{0:N" + Utility.GetFraction + "}", returnedChecks);//returnedChecks.ToString(); //MLHIDE

            //     finalBalance = finalBalance + unpayedChecks + returnedChecks;
            //     /*if (isMinus && finalBalance > 0)
            //         finalBalance = finalBalance * -1;*/
            //     txtBalance.Text = String.Format("{0:N" + Utility.GetFraction + "}", finalBalance);//finalBalance.ToString(); //MLHIDE
            //     #endregion الرصيد بعد اضافة رصيد الشيكات
            // }
            return res;
        }
        public static async Task<dynamic> GetDataShap3(IDbConnection db, IDbTransaction trans, GetStatmentOfAccountsRptShap3F.Query req)
        {
            try
            {
                req.prevBalanceCaption = "رصيد سابق";
                // --@Int321=> @langID
                // --@Int322=> @isBaseCurr
                // --@Int323=> @accID
                // --@DateTime1=> @fromDate
                // --@DateTime2=> @toDate
                // --@Str1=> @prevBalCaption
                // --@Int324=> with invoice details=1 or not=0
                // --@Int325=> from right=1 , from left=2
                // --@Int326=> with checks details=1 or not=0
                // --@Int327=> Curr id
                // --@Int328=> Trans By Curr
                // --@Int329=> استثناء قيود اقفال الدخل والضريبة 1=استثناء، 0=عدم استثناء
                // --@Int3210=> اظهار رقم السند اليدوي في عمود الملاحظة
                // --@Int32100=> احتساب رصيد الشيكات
                // --@Str2=> client pc date format
                // --@Int3211 Prev Credit Debit
                var fromDate = req.from_date.Date; // يبدأ من 00:00:00
                var toDate = req.to_date.Date.AddDays(1).AddSeconds(-1); // يضيف يوم ويحذف ثانية = 23:59:59
                var param = new
                {
                    Int321 = req.lang_id,
                    Int322 = req.base_curr,
                    Int323 = req.account_id,
                    DateTime1 = fromDate,
                    DateTime2 = toDate,
                    Str1 = req.prevBalanceCaption ?? "",
                    Int324 = req.withInvoicesDetails,
                    Int325 = req.fromRight,
                    Int326 = req.withChecksDetails,
                    Int327 = req.currency_id,
                    Int328 = req.transByCurr,
                    Int329 = req.hideEqfal,
                    Int3210 = req.showManualNo,
                    Str2 = req.checksDateFormat,
                    Int3211 = req.prevDebitCredit,
                };



                float fromValue = float.MinValue - 10000, toValue = float.MaxValue - 10000;
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
                if (req.getCheqBalance) // رصيد الشيكات
                {
                    // Call the SP
                    var parameters = new
                    {
                        Int321 = req.currency_id,
                        DateTime1 = toDate,
                        Str1 = req.from_code,
                        Str2 = req.to_code,
                        Float1 = fromValue,
                        Float2 = toValue,
                        Int322 = req.base_curr,
                        Int323 = 1,
                        Str3 = req.accountPrefix,
                        Int324 = -1,
                        Int325 = 0,
                        Int326 = -1,
                        Int327 = -1,
                        Int328 = -1,
                        Int329 = -1,
                        Int3210 = -1,
                        Int3211 = 0,
                        DateTime2 = DateTime.Now,
                        DateTime3 = DateTime.Now
                    };
                    // Execute the stored procedure
                    var totalCheckBalance = await db.QueryAsync<dynamic>(
                        "BalancesByDateCES_Report_sp",
                        parameters,
                  transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );
                    using (var multi = await db.QueryMultipleAsync(
                  "StatementOfAccount_Report_Shap3_sp",
                  param,
                  transaction: trans,
                  commandType: CommandType.StoredProcedure))
                    {
                        // Fully materialize all results inside the using block
                        var main = (await multi.ReadAsync()).ToList();
                        var items = (await multi.ReadAsync()).ToList();
                        var checks = (await multi.ReadAsync()).ToList();
                        return new
                        {
                            main,
                            items,
                            checks,
                            totalCheckBalance
                        };
                    }
                }
                using (var multi = await db.QueryMultipleAsync(
                    "StatementOfAccount_Report_Shap3_sp",
                    param,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure))
                {
                    // Fully materialize all results inside the using block
                    var main = (await multi.ReadAsync()).ToList();
                    var items = (await multi.ReadAsync()).ToList();
                    var checks = (await multi.ReadAsync()).ToList();
                    return new
                    {
                        main,
                        items,
                        checks,
                        totalCheckBalance = (decimal?)null
                    };
                }
            }
            catch (Exception EX)
            {
                throw;
            }
        }

    }
}