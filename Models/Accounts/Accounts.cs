using System.Data;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Utilities;
using System.Text;
using SHLAPI.Features.Accounts;
using SHLAPI.Models.VoucherQabdSarf;

namespace SHLAPI.Models.Accounts
{
    public class Accounts_M
    {
        public int id { get; set; }
        public string no { get; set; }
        public string name { get; set; }
        public static async Task<IEnumerable<Accounts_M>> GetData(IDbConnection db, IDbTransaction trans, GetAccountsF.Query req)
        {
            try
            {

                string sqlStat = "";
                if (req.currency_id > 0) sqlStat += @" and ChartOfAccount.currency_id={0}";
                if (req.from_code != null && req.from_code.Trim().Length > 0) sqlStat += @" and ChartOfAccount.no>={1}";
                if (req.to_code != null && req.to_code.Trim().Length > 0) sqlStat += @" and ChartOfAccount.no<={2}";
                if (req.accountPrefix != null && req.accountPrefix.Trim().Length > 0) sqlStat += @" and ChartOfAccount.Code='{3}'";
                string where = string.Format(sqlStat, req.currency_id, req.from_code, req.to_code, req.accountPrefix);

                string spName = "ChartOfAccount_SOA__sp";
                if (req.accountPrefix == "C" || req.accountPrefix == "S") spName = "ChartOfAccount_SOA_WithDelegates_sp";
                var param = new
                {
                    where
                };
                var res = await db.QueryAsync<Accounts_M>(
                     spName,
                     param,
                     transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
                var orderedRes = res.OrderBy(r => r.no).ToList();
                return orderedRes;
            }
            catch (Exception EX)
            {

            }
            return null;
        }
        public static async Task<IEnumerable<dynamic>> GetAccountBalance(IDbConnection db, GetAccountBalanceF.Query request, IDbTransaction trans)
        {
            // --@Int321 int=> CurrID ,@Int322=> isBaseCurr ,@DateTime1 datetime=> ToDate
            // --@Str1 nvarchar(20)=> fromAccount ,@Str2 nvarchar(20)=> toAccount
            // --@Float1=> fromValue ,@Float2=> toValue
            // --@Int323=> حسابات العملة المختارة فقط
            // --@Int324=> استثناء قيود اقفال قائمة الدخل واقفال الضريبة، 1= استثناء، 0= عدم استثناء
            var result = await db.QueryAsync<dynamic>(
                            "BalancesByDate_Report_sp",
                            new
                            {
                                Int321 = request.currencyId,
                                DateTime1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59),
                                Str1 = request.fromAccount,
                                Str2 = request.toAccount,
                                Float1 = request.fromValue,
                                Float2 = request.toValue,
                                Int322 = request.isBaseCurr,
                                Int323 = request.withSelectedCurrency,
                                Int324 = 0
                            },
                            commandType: CommandType.StoredProcedure,
                                transaction: trans
                            );
            var list = result.ToList();
            return list;
        }

public class AccountBalanceResult
{
    public bool success { get; set; }
    public char debitOrCredit { get; set; }
    public double accountRasid { get; set; }
}
       public static async Task<AccountBalanceResult> CalcAccountRasid(IDbConnection db, IDbTransaction trans,int accountId, int accountCurrency)
        {
                            AccountBalanceResult returnObj = new AccountBalanceResult();
            try
            {
                double debitAmount = 0;
                double creditAmount = 0;
                // Debit
                var debitSql = @"SELECT sum(account_equal_amount) as Actual_Amount
                             FROM Journals 
                             WHERE account_id = @accountId 
                               AND Debit_credit = 'D' 
                               AND Account_Curr = @accountCurrency 
                               AND status = '0'";

                debitAmount = await db.ExecuteScalarAsync<double?>(debitSql, new { accountId, accountCurrency },trans) ?? 0;

                // Credit
                var creditSql = @"SELECT sum(account_equal_amount) as Actual_Amount
                              FROM Journals 
                              WHERE account_id = @accountId 
                                AND Debit_credit = 'C' 
                                AND Account_Curr = @accountCurrency 
                                AND status = '0'";

                creditAmount = await db.ExecuteScalarAsync<double?>(creditSql, new { accountId, accountCurrency },trans) ?? 0;

                char debitOrCredit = debitAmount > creditAmount ? 'D' : 'C';
                double accountRasid = debitAmount - creditAmount;


                returnObj.success = true;
                returnObj.debitOrCredit = debitOrCredit;
                returnObj.accountRasid = accountRasid;

            }
            catch (Exception EX)
            {
                throw;
            }
            return returnObj;
        }
   
    }
    public class SearchAccount_M
    {
    
﻿     public static async Task<IEnumerable<dynamic>> SearchAccounts(IDbConnection db, IDbTransaction trans,SearchAccountsF.Query query)
        {
            try
            {
                string _fillter = " where 1=1 ";                      //MLHIDE
                if (query.additionalConditions !=null && query.additionalConditions.Trim() != "")
                {
                    _fillter += " and " + query.additionalConditions;      //MLHIDE 
                }
                string Account_Name = query.accountName ==null ?"":query.accountName ;//.Trim();
                bool IsTotalSearch = true;
                bool IsSearchFromBegin = false;
                bool IsSearchFromEnd = false;
                bool IsSearchForAll = true;
                bool _doColumnNo = true;
                #region Old View
                if (!query.newViewProp)
                {
                    string s = query.accountName != null ?query.accountName : "";
                    string[] ArrOfAccount_Name = Account_Name.Trim().Split(' ');
                    if (ArrOfAccount_Name.Length != 0)
                    {
                        if (Account_Name.Trim() != "")
                        {
                            if (_fillter.Contains("where"))           // //MLHIDE
                            {
                                _fillter += " and";                   // //MLHIDE
                            }
                            else
                            {
                                _fillter += " where";                 // //MLHIDE
                            }
                        }
                        for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                        {
                            if (ArrOfAccount_Name[i].Trim() == "")
                            {
                                continue;
                            }
                            if (!IsTotalSearch)
                            {
                                _fillter += "  (Replace(name_DynSearch,'ه','ة')  like "; // //MLHIDE
                            }
                            else
                            {
                                _fillter += "  (Replace(name_DynSearch,'ه','ة')  like "; // //MLHIDE
                            }
                            if (IsSearchFromBegin)
                            {
                                _fillter += " Replace(N'" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'" + ArrOfAccount_Name[i] + "%'"; // //MLHIDE

                            }
                            else if (IsSearchFromEnd)
                            {
                                _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "') "; // //MLHIDE
                            }
                            else if (IsSearchForAll)
                            {
                                _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "%') "; // //MLHIDE
                            }

                            if (ArrOfAccount_Name.Length > 1 && i + 1 < ArrOfAccount_Name.Length)
                            {
                                _fillter += " and  ";                  // //MLHIDE
                            }
                        }
                    }
                }
                #endregion
                #region New View
                if (query.newViewProp)
                {
                    string s = query.accountName != null ? query.accountName:"";
                    string[] ArrOfAccount_Name = Account_Name.Trim().Split(' ');
                    if (ArrOfAccount_Name.Length != 0)
                    {
                        if (Account_Name.Trim() != "")
                        {
                            if (_fillter.Contains("where"))            // //MLHIDE
                            {
                                _fillter += " and";                    // //MLHIDE
                            }
                            else
                            {
                                _fillter += " where";                  // //MLHIDE
                            }
                        }
                        for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                        {
                            if (ArrOfAccount_Name[i].Trim() == "")
                            {
                                continue;
                            }
                            if (!IsTotalSearch)
                            {
                                _fillter += "  (Replace(name,'ه','ة')  like "; // //MLHIDE
                            }
                            else
                            {
                                _fillter += "  (Replace(name,'ه','ة')  like "; // //MLHIDE
                            }
                            if (IsSearchFromBegin)
                            {
                                _fillter += " Replace(N'" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'" + ArrOfAccount_Name[i] + "%'"; // //MLHIDE

                            }
                            else if (IsSearchFromEnd)
                            {
                                _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "') "; // //MLHIDE
                            }
                            else if (IsSearchForAll)
                            {
                                _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "%') "; // //MLHIDE
                            }

                            if (ArrOfAccount_Name.Length > 1 && i + 1 < ArrOfAccount_Name.Length)
                            {
                                _fillter += " and  ";                  // //MLHIDE
                            }
                        }
                    }
                }
                #endregion
                if (query.isFillteredByStopped)
                {
                    if (_fillter.Trim() != "")
                        _fillter += " and ";                           //MLHIDE
                    else _fillter += " where ";                        //MLHIDE
                    _fillter += " stop_transactions='false' ";         //MLHIDE
                }
                if (query.isAccount)
                {
                    _fillter += " and code ='A'";
                }
                else
                {
                    _fillter += " and code <> 'A'";
                }
                if (_doColumnNo)
                    _fillter += " order by no";                            // //MLHIDE

                var dtAccountSearch = await VoucherQabdSarfGetData_M.GetByDynamicSearchSp(db, query.tableName, _fillter, "*", trans);
                return dtAccountSearch;
            }
            catch (Exception EX)
            {
                throw;
            }
        }



        //        private void OpenSearchformWithF10(string _where = "")
        // {
        //     try
        //     {
        //         frmSearch search = new frmSearch("QabdAccount_v", ml.ml_string(2652, "بحث"), _where, false, false, false, false, true, true); // //MLHIDE
        //         search.ShowDialog(this);
        //         if (search._primaryKey != null)
        //         {
        //             BusinessLayer.ChartOfAccount obj_AccountData = new BusinessLayer.ChartOfAccount((int)search._primaryKey, con, true, Utility.UserId);
        //             PrevPersonAccountNO = txtPersonAccountNo.Text;
        //             txtPersonAccountNo.ISCO_MemberValue = search._primaryKey.ToString();
        //             personId = (int)search._primaryKey;
        //             txtMVNo.Focus();
        //             txtPersonAccountNo_Leave_1(new object(), new EventArgs());
        //             PrevPersonAccountNO = "";
        //             if (obj_AccountData.Code != "A") VisibledCustomerRased(true); // //MLHIDE
        //             else VisibledCustomerRased(false);
        //         }
        //     }
        //     catch (Exception EX)
        //     {
        //         ChangeNotification(MsgType.Error, EX.StackTrace.ToString());
        //         Utility.InsertException(EX, con);
        //     }
        // }
    }
}