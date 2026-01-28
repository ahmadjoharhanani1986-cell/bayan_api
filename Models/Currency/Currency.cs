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
            DateTime nowDate = _date;
            nowDate = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, 00, 00, 00);
            nowDate = DateTime.SpecifyKind(nowDate, DateTimeKind.Utc);
            var param = new
            {
                dateTime1 = nowDate
            };
            var res = await db.QueryAsync<int>(
                 "Copy_Rates_sp",
                 param,
                 transaction: trans,
                commandType: CommandType.StoredProcedure
            );
            if (res != null && res.AsList().Count > 0 && res.AsList()[0]==1)
                return true;

            return false;
        }

        public static async Task<double> GetCurrencyExchangePrice(IDbConnection db, IDbTransaction trans, int currencyId, DateTime _date)
        {
            DateTime nowDate = _date;
       
            nowDate = new DateTime(nowDate.Year, nowDate.Month, nowDate.Day, 00, 00, 00);
            string sqlDate = nowDate.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            string where = " where currency_id=" + currencyId + " and date='" + sqlDate + "' ";
            var param = new
            {
                where
            };
            try
            {
                var res = await db.QueryAsync<double>(
                     "Currency_Price_sp",
                     param,
                     transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
                            if (res != null && res.AsList().Count > 0)
                return res.AsList()[0];
            }
            catch (Exception EX)
            {
                throw;
            }
            return 0;
        }
        public static async Task<IEnumerable<dynamic>> GetUsersTreasuryRightsBoxes(IDbConnection db, IDbTransaction trans, int currencyId, int userId)
        {
            string where = " 1=1 and Users_Treasury_Rights.currency_id=" + currencyId + " and  Users_Treasury_Rights.user_id=" + userId + "";
            var param = new
            {
                where
            };
            try
            {
                var res = await db.QueryAsync<dynamic>(
                     "GetUsers_Treasury_RightsAllData_sp",
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
}