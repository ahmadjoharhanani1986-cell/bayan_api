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
public static async Task<IEnumerable<Accounts_M>> GetData(
    IDbConnection db,
    IDbTransaction trans,
    GetAccountsF.Query req)
{
    try
    {
        string sql = @"
        SELECT 
            id,
            no,
            name,
            '' AS address,
            '' AS mobile
        FROM ChartOfAccount
        WHERE 
            (Code <> 'A' 
            OR (Code = 'A' AND id NOT IN (
                SELECT parent_account_id 
                FROM ChartOfAccount 
                WHERE parent_account_id <> id
            )))
        ";

        var param = new DynamicParameters();

        if (req.currency_id > 0)
        {
            sql += " AND currency_id = @currency_id ";
            param.Add("@currency_id", req.currency_id);
        }

        if (!string.IsNullOrEmpty(req.from_code))
        {
            sql += " AND no >= @from_code ";
            param.Add("@from_code", req.from_code);
        }

        if (!string.IsNullOrEmpty(req.to_code))
        {
            sql += " AND no <= @to_code ";
            param.Add("@to_code", req.to_code);
        }

        if (!string.IsNullOrEmpty(req.accountPrefix))
        {
            sql += " AND Code = @prefix ";
            param.Add("@prefix", req.accountPrefix);
        }

        sql += " ORDER BY no ASC;";

        var res = await db.QueryAsync<Accounts_M>(
            sql,
            param,
            transaction: trans,
            commandType: CommandType.Text
        );

        return res.ToList();
    }
    catch
    {
        throw;
    }
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
    public static async Task<IEnumerable<dynamic>> SearchAccounts(
    IDbConnection db, 
    IDbTransaction trans, 
    SearchAccountsF.Query query)
{
    var sql = new StringBuilder("SELECT * FROM " + query.tableName + " WHERE 1=1 ");
    var parameters = new DynamicParameters();

    // Additional conditions (⚠ يجب التأكد أنها آمنة أو من النظام فقط)
    if (!string.IsNullOrWhiteSpace(query.additionalConditions))
        sql.Append(" AND " + query.additionalConditions);

    // Search name
    if (!string.IsNullOrWhiteSpace(query.accountName))
    {
        var words = query.accountName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            string paramName = $"@name{i}";
            sql.Append($" AND (REPLACE(name,'ه','ة') LIKE {paramName} OR no LIKE {paramName})");
            parameters.Add(paramName, $"%{words[i]}%");
        }
    }

    // Filter stopped accounts
    if (query.isFillteredByStopped)
        sql.Append(" AND stop_transactions = 0");

    // Account type
    if (query.isAccount)
        sql.Append(" AND code = 'A'");
    else
        sql.Append(" AND code <> 'A'");

    // Order
    sql.Append(" ORDER BY no");

    return await db.QueryAsync<dynamic>(sql.ToString(), parameters, transaction: trans);
}

// ﻿     public static async Task<IEnumerable<dynamic>> SearchAccounts(IDbConnection db, IDbTransaction trans,SearchAccountsF.Query query)
//         {
//             try
//             {
//                 string _fillter = " where 1=1 ";                      //MLHIDE
//                 if (query.additionalConditions !=null && query.additionalConditions.Trim() != "")
//                 {
//                     _fillter += " and " + query.additionalConditions;      //MLHIDE 
//                 }
//                 string Account_Name = query.accountName ==null ?"":query.accountName ;//.Trim();
//                 bool IsTotalSearch = true;
//                 bool IsSearchFromBegin = false;
//                 bool IsSearchFromEnd = false;
//                 bool IsSearchForAll = true;
//                 bool _doColumnNo = true;
//                 #region Old View
//                 if (!query.newViewProp)
//                 {
//                     string s = query.accountName != null ?query.accountName : "";
//                     string[] ArrOfAccount_Name = Account_Name.Trim().Split(' ');
//                     if (ArrOfAccount_Name.Length != 0)
//                     {
//                         if (Account_Name.Trim() != "")
//                         {
//                             if (_fillter.Contains("where"))           // //MLHIDE
//                             {
//                                 _fillter += " and";                   // //MLHIDE
//                             }
//                             else
//                             {
//                                 _fillter += " where";                 // //MLHIDE
//                             }
//                         }
//                         for (int i = 0; i < ArrOfAccount_Name.Length; i++)
//                         {
//                             if (ArrOfAccount_Name[i].Trim() == "")
//                             {
//                                 continue;
//                             }
//                             if (!IsTotalSearch)
//                             {
//                                 _fillter += "  (Replace(name_DynSearch,'ه','ة')  like "; // //MLHIDE
//                             }
//                             else
//                             {
//                                 _fillter += "  (Replace(name_DynSearch,'ه','ة')  like "; // //MLHIDE
//                             }
//                             if (IsSearchFromBegin)
//                             {
//                                 _fillter += " Replace(N'" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'" + ArrOfAccount_Name[i] + "%'"; // //MLHIDE

//                             }
//                             else if (IsSearchFromEnd)
//                             {
//                                 _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "') "; // //MLHIDE
//                             }
//                             else if (IsSearchForAll)
//                             {
//                                 _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "%') "; // //MLHIDE
//                             }

//                             if (ArrOfAccount_Name.Length > 1 && i + 1 < ArrOfAccount_Name.Length)
//                             {
//                                 _fillter += " and  ";                  // //MLHIDE
//                             }
//                         }
//                     }
//                 }
//                 #endregion
//                 #region New View
//                 if (query.newViewProp)
//                 {
//                     string s = query.accountName != null ? query.accountName:"";
//                     string[] ArrOfAccount_Name = Account_Name.Trim().Split(' ');
//                     if (ArrOfAccount_Name.Length != 0)
//                     {
//                         if (Account_Name.Trim() != "")
//                         {
//                             if (_fillter.Contains("where"))            // //MLHIDE
//                             {
//                                 _fillter += " and";                    // //MLHIDE
//                             }
//                             else
//                             {
//                                 _fillter += " where";                  // //MLHIDE
//                             }
//                         }
//                         for (int i = 0; i < ArrOfAccount_Name.Length; i++)
//                         {
//                             if (ArrOfAccount_Name[i].Trim() == "")
//                             {
//                                 continue;
//                             }
//                             if (!IsTotalSearch)
//                             {
//                                 _fillter += "  (Replace(name,'ه','ة')  like "; // //MLHIDE
//                             }
//                             else
//                             {
//                                 _fillter += "  (Replace(name,'ه','ة')  like "; // //MLHIDE
//                             }
//                             if (IsSearchFromBegin)
//                             {
//                                 _fillter += " Replace(N'" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'" + ArrOfAccount_Name[i] + "%'"; // //MLHIDE

//                             }
//                             else if (IsSearchFromEnd)
//                             {
//                                 _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "') "; // //MLHIDE
//                             }
//                             else if (IsSearchForAll)
//                             {
//                                 _fillter += " Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') or no like N'%" + ArrOfAccount_Name[i] + "%') "; // //MLHIDE
//                             }

//                             if (ArrOfAccount_Name.Length > 1 && i + 1 < ArrOfAccount_Name.Length)
//                             {
//                                 _fillter += " and  ";                  // //MLHIDE
//                             }
//                         }
//                     }
//                 }
//                 #endregion
//                 if (query.isFillteredByStopped)
//                 {
//                     if (_fillter.Trim() != "")
//                         _fillter += " and ";                           //MLHIDE
//                     else _fillter += " where ";                        //MLHIDE
//                     _fillter += " stop_transactions='false' ";         //MLHIDE
//                 }
//                 if (query.isAccount)
//                 {
//                     _fillter += " and code ='A'";
//                 }
//                 else
//                 {
//                     _fillter += " and code <> 'A'";
//                 }
//                 if (_doColumnNo)
//                     _fillter += " order by no";                            // //MLHIDE

//                 var dtAccountSearch = await VoucherQabdSarfGetData_M.GetByDynamicSearchSp(db, query.tableName, _fillter, "*", trans);
//                 return dtAccountSearch;
//             }
//             catch (Exception EX)
//             {
//                 throw;
//             }
//         }



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