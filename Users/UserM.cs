using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Models.Authentication;
using SHLAPI.Utilities;

namespace SHLAPI.Models
{
    public class UserM
    {
        public UserM()
        {

        }

        public int id { get; set; }

        public string name { get; set; }

        public string login_name { get; set; }

        public int main_role { get; set; }

        public string pwd { get; set; }

        public string sault { get; set; }

        public string token { get; set; }

        public string email { get; set; }

        public string mobile { get; set; }

        public int is_system_admin { get; set; }

        public int status { get; set; }

        public bool can_change_role { get; set; }

        public int language_id { get; set; }


        public static async Task<Result> Save(IDbConnection db, IDbTransaction trans, UserM obj)
        {
            Result result = new Result();
            string cmd = "";
            if (obj.id == 0)
            {
                obj.sault = StringUtil.RandomString(9);
                obj.pwd = SHA256.GetHashSHA256(obj.pwd + obj.sault);
                cmd =
                    @"INSERT INTO users_tbl (
                    name
                    ,login_name
                    ,main_role
                    ,sault
                    ,pwd
                    ,email
                    ,mobile
                    ,is_system_admin
                    ,status
                    ,can_change_role
                    ,language_id
                    )VALUES(
                    @name
                    ,@login_name
                    ,@main_role
                    ,@sault
                    ,@pwd
                    ,@email
                    ,@mobile
                    ,@is_system_admin
                    ,@status
                    ,@can_change_role
                    ,@language_id
                    )";
            }
            else
            {
                cmd =
                    @"UPDATE users_tbl
                    SET
                    name = @name
                    ,login_name = @login_name
                    ,main_role = @main_role
                    ,pwd = @pwd
                    ,email = @email
                    ,mobile = @mobile
                    ,is_system_admin = @is_system_admin
                    ,status = @status
                    ,can_change_role=@can_change_role
                    ,language_id=@language_id
                    WHERE id=@id";
            }
            await db.ExecuteAsync(cmd, obj, trans);
            if (obj.id == 0)
            {
                obj.id = await CommonM.GetLastId(db, trans);
            }
            return result;
        }

        static internal async Task<bool> CheckIfDisabled(IDbConnection db, IDbTransaction trans, int userId)
        {
            string selectStatment = @" select count(*) from users_tbl where id=@user_id and status=3";
            var res = await db.ExecuteScalarAsync<int>(selectStatment, new { user_id=userId });
            return res == 1;
        }

        internal static async Task<Result> ChangePassword(IDbConnection db, IDbTransaction trans, int userId, string password)
        {
            Result result=new Result();

            string selectUser = " select sault from users_tbl where id=@user_id";
            var sault = await db.ExecuteScalarAsync<string>(selectUser, new { user_id=userId });
            if (string.IsNullOrEmpty(sault))
                return result.Fail(new ErrorDescription(){id=(int)ErrorReason.UserHasNoSault,description="User has no sault"});

            string selectStatment = @" update users_tbl set pwd=@passwrod where id=@user_id";
            string hashedPWD = SHA256.GetHashSHA256(password + sault);
            await db.ExecuteAsync(selectStatment, new { user_id=userId, passwrod = hashedPWD });
            
            return result;
        }

        internal static async Task<Result> CheckPassword(IDbConnection db, IDbTransaction trans, int userId, string password)
        {
            Result result=new Result();

            string selectUser = " select sault from users_tbl where id=@user_id";
            var sault = await db.ExecuteScalarAsync<string>(selectUser, new { user_id=userId });
            if (string.IsNullOrEmpty(sault))
                return result.Fail(new ErrorDescription(){id=(int)ErrorReason.UserHasNoSault,description="User has no sault"});

            string selectStatment = @" select u.id,u.name as user_name,u.status,u.main_role,u.token,r.name main_role_name
                                    ,u.language_id
                                    from users_tbl u 
                                    left join roles_tbl r on r.id=u.main_role 
                                    where u.id=@user_id and u.pwd=@pwd";
            string hashedPWD = SHA256.GetHashSHA256(password + sault);
            var res = await db.QueryAsync<Authentication_M>(selectStatment, new { user_id=userId, pwd = hashedPWD });
            List<Authentication_M> list = res.AsList();
            if (list.Count == 1)
            {
                if (list[0].status != 1)
                {
                    result.Fail(new ErrorDescription(){id=(int)ErrorReason.UserIsDisabled,description="User is disabled"});
                }
            }
            else
            {
                result.Fail(new ErrorDescription(){id=(int)ErrorReason.InvalidPassword,description="Invalid password"});
            }

            return result;
        }

        internal static async Task<Result> UpdateLanguage(IDbConnection db, IDbTransaction trans, int user_id, int language_id)
        {
            Result result = new Result();
            string cmd = @"UPDATE users_tbl
                            SET
                            language_id=@language_id
                            WHERE id=@id";

            CommonM.AdjustQuery(cmd);
            await db.ExecuteAsync(cmd,new {id=user_id,language_id=language_id},trans);
            return result;
        }

        internal static async Task<Result> Delete(IDbConnection db, IDbTransaction trans, int user_id)
        {
            Result result = new Result();
            string cmd = @"UPDATE users_tbl
                            SET
                            status=3
                            WHERE id=@id";

            CommonM.AdjustQuery(cmd);
            await db.ExecuteAsync(cmd,new {id=user_id},trans);
            return result;
        }

        public static async Task<bool> IsTokenValid(IDbConnection db, IDbTransaction trans, int userId, string token)
        {
            string selectStatment = @" select count(*) from User_Profile where id=@user_id and token=@token";
            var res = await db.ExecuteScalarAsync<int>(selectStatment, new { user_id=userId, token = token });
            return res == 1;
        }

        internal static async Task<bool> IsPM(IDbConnection db, IDbTransaction trans, int userId)
        {
            string selectStatment = @" select count(*) from users_tbl where id=@user_id and main_role=@main_role";
            var res = db.ExecuteScalar<int>(selectStatment, new { user_id=userId ,main_role=(int)Role.project_manager});
            return res == 1;
        }

        internal static async Task<bool> IsQATeamleader(IDbConnection db, IDbTransaction trans, int userId)
        {
            string selectStatment = @" select count(*) from users_tbl where id=@user_id and main_role=@main_role";
            var res = db.ExecuteScalar<int>(selectStatment, new { user_id=userId ,main_role=(int)Role.qa_teamleader});
            return res == 1;
        }

        public static async Task<UserM> Get(IDbConnection db, IDbTransaction trans, int id, NavigationTypes nav=NavigationTypes.GetIt)
        {
            string query = @"SELECT *
                                    FROM User_Profile
                                    where user_id=@id
                                    ";
           // query = CommonM.AddNavigationQuery(query, nav);

            //CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<UserM>(query, new { id = id });
            return (UserM)CommonM.GetFirst(result);
        }

        public static async Task<UserM> GetPassword(IDbConnection db, IDbTransaction trans, int id)
        {
            string query = @"SELECT pwd
                                    FROM users_tbl u
                                    where id=@id";

            CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<UserM>(query, new { id = id });
            return (UserM)CommonM.GetFirst(result);
        }        

        public static async Task<IEnumerable<UsersQueryDTO>> GetList(IDbConnection db, IDbTransaction trans, int status)
        {
            string query = @"SELECT u.id
                                    ,u.name
                                    ,u.login_name
                                    ,u.main_role
                                    ,u.email
                                    ,u.mobile
                                    ,u.is_system_admin
                                    ,u.status
                                    ,r.name AS role_name
                                    ,us.name AS status_name
                                    FROM users_tbl u
                                    LEFT JOIN roles_tbl r ON r.id = u.main_role
                                    LEFT JOIN users_status_tbl us ON us.id = u.status
                                    where u.status != 99
                                    ";
            if (status > 0)
                query += " and u.status=@status";
            query += " order by u.id desc";

            CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<UsersQueryDTO>(query, new { status = status });
            return result;
        }

        public static async Task<IEnumerable<IdNameDTO>> GetListIdName(IDbConnection db, IDbTransaction trans, int status)
        {
            string query = @"SELECT u.id
                                    ,u.name
                                    ,u.status
                                    FROM users_tbl u
                                    where status != 99
                                    ";
            if (status > 0)
                query += " and status=@status";
                
            CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<IdNameDTO>(query, new { status = status });
            return result;
        }

        public static async Task<IEnumerable<UserRoleDTO>> GetRolesList(IDbConnection db, IDbTransaction trans)
        {
            string query = @"SELECT id
                                	,name
                                    FROM roles_tbl
                                    ORDER BY id asc
                                    ";

            CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<UserRoleDTO>(query);
            return result;
        }
    }
}
