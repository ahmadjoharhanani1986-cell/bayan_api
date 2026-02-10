using System.Data;
using Dapper;


namespace SHLAPI.Models.Currency
{
    public class Currency_M
    {
        public int id { get; set; }
        public string no { get; set; }
        public string name { get; set; }
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans)
        {
            try
            {
                string where = " 1=1 order by id asc";
                string spName = "sp_Currency_GetAllByWhere";
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

        public static async Task<dynamic> CheckUserTresuryForAccount(IDbConnection db, IDbTransaction trans, int accountId)
        {

            if (accountId != 0)
            {

                string where = " id=" + accountId;
                string spName = "sp_ChartOfAccount_GetAllByWhere";
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
                if (res != null && res.AsList().Count > 0)
                    return res.AsList()[0];



                // if (coaObj == null) return false;
                // if (coaObj.Currency_Id != (int)cmbCurrency.SelectedValue)
                // {
                //     if (coaObj.Different_Currency_Trans == "1") // //MLHIDE
                //     {
                //         if (ISCO_MessageBox.Show(this, Utility.GetMessage(2546, ml.ml_string(2631, "لا يمكنك ادخال سند قبض بعمله تختلف عن عمله الحساب الدائن. هل تود تغيير عمله سند القبض حسب عمله الحساب")), Utility.ShamelMessage, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == System.Windows.Forms.DialogResult.Yes)
                //         {
                //             cmbCurrency.SelectedValue = coaObj.Currency_Id;
                //         }
                //         else
                //         {
                //             txtCRAccountNo.Focus();
                //             return false;
                //         }
                //     }
                //     else if (coaObj.Different_Currency_Trans == "2") // //MLHIDE
                //     {
                //         ChangeNotification(MsgType.WarningOrNotify, Utility.GetMessage(406, ml.ml_string(2632, "انتبه عمله سند القبض تختلف عن عمله الحساب الدائن. اختلاف العملتين مسموح")));
                //     }
                //     else if (coaObj.Different_Currency_Trans == "3") // //MLHIDE
                //     {
                //         // Settings.DifferentAccounts = “3” يسمح بذلك ودون تنبيه
                //     }
                //     else
                //     {

                //     }
                // }
            }
            return null;
        }

    public static async Task<bool> CopyCurrenciesExchangePrice(IDbConnection db, IDbTransaction trans, DateTime _date)
{
    // Normalize the date to midnight
    DateTime nowDate = new DateTime(_date.Year, _date.Month, _date.Day, 0, 0, 0, DateTimeKind.Utc);

    // Parameters object
    var param = new
    {
        in_date = nowDate // MySQL procedure parameter name
    };

    try
    {
        // Call the MySQL procedure
        var res = await db.QueryAsync<int>(
            "Copy_Rates", // MySQL procedure name
            param,
            transaction: trans,
            commandType: CommandType.StoredProcedure
        );

        // Check if result returned 1 (inserted)
        if (res != null && res.AsList().Count > 0 && res.AsList()[0] == 1)
            return true;

        return false;
    }
    catch (Exception ex)
    {
        // Optional: log ex
        throw;
    }
}


public static async Task<double> GetCurrencyExchangePrice(IDbConnection db, IDbTransaction trans, int currencyId, DateTime date)
{
    // Normalize the date to remove time
    DateTime nowDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

    string sql = @"
        SELECT exchange_price
        FROM Currency_prices
        WHERE currency_id = @CurrencyId
          AND date = @Date
        LIMIT 1;
    ";

    var param = new
    {
        CurrencyId = currencyId,
        Date = nowDate
    };

    try
    {
        // QuerySingleOrDefaultAsync ensures we get one value or 0
        double? price = await db.QueryFirstOrDefaultAsync<double?>(sql, param, trans);
        return price ?? 0; // Return 0 if null
    }
    catch (Exception ex)
    {
        throw;
    }
}

public static async Task<IEnumerable<dynamic>> GetUsersTreasuryRightsBoxes(IDbConnection db, IDbTransaction trans, int currencyId, int userId)
{
    string sql = @"
        SELECT *
        FROM Users_Treasury_Rights
        WHERE currency_id = @CurrencyId
          AND user_id = @UserId;
    ";

    var param = new
    {
        CurrencyId = currencyId,
        UserId = userId
    };

    try
    {
        var res = await db.QueryAsync<dynamic>(
            sql,
            param,
            transaction: trans
        );
        return res;
    }
    catch (Exception ex)
    {
        throw;
    }
}

   
   
   
    }
}