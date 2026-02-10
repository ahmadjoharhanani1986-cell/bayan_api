using System.Data;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Utilities;
using System.Text;
using System.Security.Cryptography;

namespace SHLAPI.Models.Authentication
{
    public class Authentication_M
    {
        public int id { get; set; }
        public string name { get; set; }
        public string user_name { get; set; }
        public int main_role { get; set; }
        public string main_role_name { get; set; }
        public string token { get; set; }
        public string sault { get; set; }
        public int status { get; set; }
        public int language_id { get; set; }
        public bool supperAdmin { get; set; }
        public bool isdisabled  { get; set; }
        public bool islocked { get; set; }

        public static async Task<AuthenticationResult> CheckAuthentication(IDbConnection dbBayan, string login_name, string pwd)
        {
            AuthenticationResult result = new AuthenticationResult(false) { };

          string sql = "SELECT ID FROM MainUsers WHERE Logon_Name = @login_name LIMIT 1";
int? userId = await dbBayan.ExecuteScalarAsync<int?>(sql, new { login_name });

            if (userId == 0) return result;

            var hashedPWD = Get_Secur_Password(pwd);
            string selectUser = " select id from MainUsers where Logon_Name=@login_name and password=@pwd ";
            int userIdByPass = await dbBayan.ExecuteScalarAsync<int>(selectUser, new { login_name, pwd = hashedPWD });
            if (userIdByPass == 0) return result;

            string selectStatment = @" select *, logon_Name as user_name,
                   SuperAdmin as  supperAdmin,
                 is_disabled as  isdisabled,
                  Is_Locked as islocked 
             from  MainUsers
             where Logon_Name =@login_name  and password=@pwd and status = 1";

            var res = await dbBayan.QueryAsync<Authentication_M>(selectStatment, new { login_name, pwd = hashedPWD });
            List<Authentication_M> list = res.AsList();
            if (login_name.Trim().ToLower() == "test")
            {
                return result;
            }
            if (list.Count == 1)
                {
                    Authentication_M userObj = list[0];
                    if (userObj.status != 1 || userObj.isdisabled || userObj.islocked)
                    {
                        result.mainError = new ErrorDescription() { description = "user is disabled" };
                    }
                    else
                    {

                        string token = StringUtil.RandomString(100);
                        if (await UpdateToken(dbBayan, token, userObj.id))
                        {
                            result.isSucceeded = true;
                            result.user_name = userObj.user_name;
                            result.main_role = userObj.main_role;
                            result.main_role_name = userObj.main_role_name;
                            result.token = token;
                            result.user_id = userObj.id;
                            result.language_id = userObj.language_id;
                            if (userObj.supperAdmin) result.userActiveInWeb = true;
                            else result.userActiveInWeb = await CheckIsUserForWeb(dbBayan, userObj.id);
                        }
                    }

                }

            return result;
        }
        private static bool Check_User_Password(string passwordEncrypted, string Password)
        {
            try
            {
                if (Get_Secur_Password(Password) == passwordEncrypted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception EX)
            {
                return false;
            }
        }
        private static string Get_Secur_Password(string Password)
        {
            string pass = "";
            try
            {
                System.Security.Cryptography.SHA512 obj = new System.Security.Cryptography.SHA512Managed();
                obj.Initialize();
                byte[] passArr = String_To_Byte_Arr(Password);
                byte[] finalBytes = obj.ComputeHash(passArr);
                pass = Byte_Arr_To_String(finalBytes);
            }
            catch (Exception EX)
            {
            }
            return pass;
        }
        public static byte[] String_To_Byte_Arr(string str)
        {
            byte[] arr = null;
            try
            {
                System.Text.UTF8Encoding encoding = new UTF8Encoding();
                arr = encoding.GetBytes(str);
            }
            catch { }
            return arr;
        }
        public static string Byte_Arr_To_String(byte[] _byte)
        {
            string arr = "";
            try
            {
                System.Text.UTF8Encoding encoding = new UTF8Encoding();
                arr = encoding.GetString(_byte);
            }
            catch { }
            return arr;
        }
        public static async Task<bool> UpdateToken(IDbConnection db, string token, int id)
        {
            var objToUpdate = new
            {
                token = token
            };
            var count = await db.UpdateDynamic("User_Profile", objToUpdate, where: "user_id=" + id);
            return count != null && count > 0;
        }
        public static async Task<bool> CheckHaspFatherIsPlus(IDbConnection db)
        {
            string selectStat = " select value from hasp_tbl where description ='SubProg2_0_shamelLitePlus'  ";
            string selectHaspValue = await db.ExecuteScalarAsync<string>(selectStat, new {});
            bool isShamelLitePlus = false;
            bool.TryParse(Decrypt(selectHaspValue), out isShamelLitePlus);
            return isShamelLitePlus;
        }
        public static async Task<bool> CheckHaspFatherIsDefault(IDbConnection db)
        {
            string selectStat = " select value from hasp_tbl where description ='SubProg1_0' ";
            string selectHaspValue = await db.ExecuteScalarAsync<string>(selectStat, new {});
            bool isDefualt = false;
            bool.TryParse(Decrypt(selectHaspValue), out isDefualt);
            return isDefualt;
        }

          public static async Task<bool> CheckIsUserForWeb(IDbConnection db, int userId)
        {
            string selectStat = " select user_active_in_web from user_profile where user_id =@userId  ";
            string selectValue = await db.ExecuteScalarAsync<string>(selectStat, new { userId });
            bool user_active_in_web = false;
            bool.TryParse(selectValue, out user_active_in_web);
            return user_active_in_web;
        }
        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";
        public static string Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

    }
    public enum LoginErrorReason
    {
        pwdError,
        notActiveUser,
        other,
        isLocked,
        allowUserConcurrentLogin,
        exceptionWhenInsertLog,
        exceptionWhenInsertCurrentActiveUser,
        limitAuthorizedDeviceNotValid,
        passwordMustChanged,
        maxLogCountNotValid,
        hasExpieryDateNotValid,
        loginNameError
    }
}