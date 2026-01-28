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
                string where = $" user_id = {userId}";
                string spName = "sp_User_Profile_GetAllByWhere";
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
             from  Security_000123.Users
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