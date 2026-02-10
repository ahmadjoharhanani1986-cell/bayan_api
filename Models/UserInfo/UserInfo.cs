using System.Data;
using Dapper;
using SHLAPI.Models.Authentication;
namespace SHLAPI.Models.UserInfo
{
    public class UserInfo_M
    {
 public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int userId)
{
    try
    {
        string sql = @"
            SELECT 
                id, user_id, open_new_tab, load_all_data, on_space_or_backspace, include_tax_onPriceEntry,
                on_text_change, open_as_float_window, can_update_pay_price, Open_ScreenMoreThanOneTime, purchase_discount,
                sell_discount, CanChangeSellPrice, appeare_item_cost, appeare_item_cost_sell, appeare_item_cost_pos,
                user_name, expand_item_space, OpenOperatrionWindow, DefaultOperation, can_edit_pay_bill, limit_debit,
                appeare_invoice_search_pos, discount_limit_percent, dashboards_refresh, appeare_internal_transfer_screen,
                check_backup_when_colsed, update_Invoice_date, can_move_dock_control, dont_alert_when_unit_price_less_than_pay,
                visible_purchase_price_in_item, com_port_name, dontAddItemWhenNotHaveBarCode, token, chkVisibleSearchCashAccountQabd
            FROM User_Profile
            WHERE user_id = @userId";

        var res = await db.QueryAsync<dynamic>(
            sql,
            new { userId },
            transaction: trans
        );

        return res;
    }
    catch (Exception ex)
    {
        throw;
    }
}

        public static async Task<IEnumerable<dynamic>> GetUserDashboards(IDbConnection db, int userId,
           IDbTransaction? trans = null)
        {
            string where = string.Format(" Users_Dashboards.permission = 'true' and Users_Dashboards.user_id ={0}", userId);
            var result = await db.QueryAsync<dynamic>(
                "UsersDashBoards_GetAllByWhere_sp", // اسم الـ SP
                new
                {
                    Str1 = where
                },
                commandType: CommandType.StoredProcedure,
                transaction: trans
            );

            return result.ToList();
        }

        public static async Task<IEnumerable<dynamic>> GetUserProjectRights(IDbConnection db, IDbTransaction trans, int userId, string langId)
        {
            try
            {
                string selectStatment = @" select *, logon_Name as user_name,
                   SuperAdmin as  supperAdmin,
                 is_disabled as  isdisabled,
                  Is_Locked as islocked 
             from  mainUsers
             where id=@userId ";

                var res = await db.QueryAsync<Authentication_M>(selectStatment, new { userId },trans);
                List<Authentication_M> list = res.AsList();
                Authentication_M userObj = new Authentication_M();
                if (list.Count == 1)
                {
                    userObj = list[0];
                }
                if (userObj.supperAdmin)
                {
                    return null;
                }
                var result = await db.QueryAsync<dynamic>(
            "Security_000123.Get_UserProjectRights_sp",
            new
            {
                Int321 = userId,
                Int322 = 3,// object type (1=user,2=group,3=role)
                Str1 = 1,
                Str2 = 1,
                Int323 = 2
            },
            transaction: trans,
            commandType: CommandType.StoredProcedure
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