using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System;
using SHLAPI.Features.LogFile;
using System.Text;
using System.IO;
using System.IO.Compression;
using MediatR;
using SHLAPI.Models.LogFile;
using Newtonsoft.Json;

namespace SHLAPI.Models
{
    public class CommonM
    {
        static public bool HasRows(IEnumerable<int> rows)
        {
            if(rows!=null)
            {
                var _rows = rows.AsList();
                if (_rows != null && _rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public class ModelResult
        {
            public bool result;
            public int lastId;
            public int statusCode;
            public string name;
            public string no;
            public string deletedIds;
            public string errorMsg;
            public ErrorReason errorReason;
        }
        public class AccountResult
        {
            public bool result;
            public string accountNo;
        }
        public enum ValidateMessage
        {
            None = 0,
            DublicateName = 1,
            DuplicateCode = 2,
            HaveTransactions = 3,
            CodeIsSameFatherCode = 4,
            AccountIdNotActive = 5,
            SalesManAccountIdNotSelected = 6,
            SalesManISSameSupervisor = 7,
            SalesManSuperVisorsInActive = 8,
            ErrorWhenUpdateLevelNo = 9,
            CodeLengthNotEqualToSettingLength = 10,
            AccountIdSameToFatherId = 11,
            FatherAccountNotActive = 12,
            FatherAccountHaveTransactions = 13,
            AccountTypeIsNotActive = 14,
            AccountCostCenterNotActive = 15,
            AccountCostCenterNotChildren = 16,
            AccountClassificationNotActiveDetials = 17,
            AccountClassificationNotActive = 18,
            AccountClassificationISDuplicated = 19,
            AccountFinancialListNotCorrect = 20,
            AccountFinancialListNotEqualFinancialListOfFather = 21,
            AccountFinancialListAssestsNotActive = 22,
            AccountFinancialListIncomeNotActive = 23,
            AccountFinancialListKhosoomNotActive = 24,
            AccountFinancialListAssestsNotEqualFinancialListAssestsOfFather = 25,
            AccountFinancialListIncomeNotEqualFinancialListIncomeOfFather = 26,
            AccountFinancialListKhosoomNotEqualFinancialListKhosoomOfFather = 27,
            MaxBalanceAmountLessThanZero = 28,
            MaxTransactionAmountLessThanZero = 29,
            MaxTransactionAmountActionLessThanZero = 30,
            CantUpdateAccountCurrency = 31,
            CheckCustomerIdNo = 32,
            CheckCustomerSubscription = 33,
            BranchLengthError = 34,
            AccountTypeNotFinancial = 35,
            AccountCurrNotEqualBankAccountCurr = 36,
            DublicateActualBankCode = 37,
            CurrIsEmpty = 38,
            JaryAccountIsEmpty = 39,
            PayedChecksAccountIsEmpty = 40,
            TahsilAccountISEmpty = 41,
            TahsilCommissionAccountIsEmpty = 42,
            CheckItemISNotLeaf = 43,
            DuplicatedItemUnit = 44,
            DuplicatedItemBarCode = 45,
            DuplicatedItemSellingPrice = 46,
            NoSellingPriceForItem = 47,
            ItemAccountIsFather = 48,
            ItemHaveLinkedItems = 49,
            ItemParentIsNotActive = 50,
            ItemHaveItemsNotDeleted = 51,
            MainStockFreezed = 52,
            MainStockIsNotLeaf = 53,
            DuplicatedNo = 54,
            CheckItemStatus = 55,
            DuplicatedItemSize = 56,
            CheckItemSizeForChildNotEqualOne = 57,
            CheckMainItemSizesCantDeleted = 58,
            DuplicatedItemColorTaste = 59,
            CheckColorTasteForChildNotEqualOne = 60,
            CheckMainColorTasteCantDeleted = 61,
            DuplicatedItemAlternatives = 62,
            CheckAllItemAlternativesIsNotActive = 63,
            CheckAllItemLinkedIsNotActive = 64,
            CheckLocationIsNotLeaf = 65,
            CheckAllTradeMarkIsNotActive = 66,
            CheckAllItemStoreLocationActive = 67,
            CheckAllItemUnitsIsNotActive = 68,
            CheckIsMainUnitDeleted = 69,
            CheckVatClassificationISNotActive = 70,
            DateTimeAboveDateTimeNow = 71,
            CheckIfValueLessOrEqualZero = 72,
            DublicatedLoginName = 73,
            YouNotHavePermissions = 74,
            ConfirmPassNotEqualPass = 75,
            OldPasswordError = 76,
            ExceptionError = 77,
            newPassIsEmpty = 78,
            userIsLocked = 79,
            pwdError = 80,
            notActiveUser = 81,
            allowUserConcurrentLogin = 82,
            limitAuthorizedDeviceNotValid = 83,
            passwordMustChanged = 84,
            maxLogCountNotValid = 85,
            hasExpieryDateNotValid = 86,
            loginNameError = 87,
            passwordNotComplex = 88,
            passLessThanMinimumLength = 89
        }

        internal static async Task<Result> DeleteRow(IDbConnection db, IDbTransaction trans, string tableName ,int id)
        {
            Result result = new Result();
            string query = "";
            query = string.Format("update {0} set",tableName);
            query += " status=@status";
            query += " where id=@id";
            await db.ExecuteAsync(query, new { id = id, status = 99 }, trans);
            return result;            
        }

        internal static string AddNavigationQuery(string query, NavigationTypes nav, string table = "")
        {
            if(!query.ToLower().Contains(" where "))
                query += " where 1=1 ";
            if (table.Length > 0) table += ".";
            if (nav == NavigationTypes.GetIt) query += " and " + table + "id=@id";
            if (nav == NavigationTypes.Next) query += " and " + table + "id>@id";
            if (nav == NavigationTypes.Prev) query += " and " + table + "id<@id";
            if (nav == NavigationTypes.First || nav == NavigationTypes.Next) query += " order by " + table + "id";
            if (nav == NavigationTypes.Last || nav == NavigationTypes.Prev) query += " order by " + table + "id desc";
            query += " LIMIT 1";

            return query;
        }

        public static void AdjustQuery(string query)
        {
            query = query.Replace("\r\n", " ");
            query = query.Replace("\t", " ");
            query = query.Replace("   ", "    ");
            while (query.IndexOf("  ") >= 0)
            {
                query = query.Replace("  ", " ");
            }
            // File.WriteAllText("debugquery.txt", query);
            string _q = query;
            _q = _q.Replace("@company_id", "1");
            _q = _q.Replace("@lang_id", "1");
            _q = _q.Replace("@currency_id", "4");
            _q = _q.Replace("@toDate", "'2024/01/01'");
            _q = _q.Replace("@fromDate", "'2023/01/01'");
            _q = _q.Replace("@fromTime", "'00:00:00'");
            _q = _q.Replace("@toTime", "'11:59:59'");
            Console.WriteLine(_q);
        }

        internal static object GetFirst(IEnumerable<object> result)
        {
            if (result != null && result.AsList().Count > 0) return result.AsList()[0];
            return null;
        }

        public static ValidateMessage validateMessage = ValidateMessage.None;
        static public async Task<int> GetLastId(IDbConnection db, IDbTransaction trans)
        {
            int logFileId = 0;
            List<int> logFileIdList = (await db.QueryAsync<int>("SELECT LAST_INSERT_ID() as mmm", null, trans)).AsList();
            if (logFileIdList != null && logFileIdList.Count > 0)
            {
                logFileId = logFileIdList[0];
            }
            return logFileId;
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        

        public static string Search_Specific(string str, string fieldName)
        {
            if (str == null)
            {
                str = " 1=2 ";
                return str;
            }
            str = str.Trim();
            string temp = "";
            List<string> Strings = SetArabicVariants(str);

            if (Strings.Count != 1)
            {
                for (int i = 0; i < Strings.Count; i++)
                {
                    if (Strings[i].Trim() != "")
                    {
                        if (i == 0)
                            temp += " ( " + fieldName + " LIKE N'%" + Strings[i] + "%'"; // //MLHIDE
                        else if (i == Strings.Count - 1)
                            temp += " OR " + fieldName + " LIKE N'%" + Strings[i] + "%')"; // //MLHIDE
                        else
                            temp += " OR " + fieldName + " LIKE N'%" + Strings[i] + "%'"; // //MLHIDE
                    }
                    else

                        temp += ") AND( 1=2 ";                    // //MLHIDE
                }
            }
            else
            {
                if (Strings[0].Trim() != "")
                    temp += fieldName + " LIKE N'%" + Strings[0] + "%'"; // //MLHIDE
                else
                    temp = " 1=1 ";                               // //MLHIDE
            }
            return temp;
        }
        
        public static bool SaveAttachmentsToFolder(string fileName, string innerPath, string fileId, byte[] attachment)
        {
            try
            {
                string extentions = "";
                if (fileName.Trim().Contains("."))
                {
                    extentions = fileName.Split(".")[1];
                }
                string attachPath = SHLAPI.Utilities.StringUtil.SystemAttachmentsFolderPath + "\\" + innerPath + "\\" + fileId + "." + extentions;
                File.WriteAllBytes(attachPath, attachment);
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public static bool DeleteAttachmentsToFolder(string fileName, string innerPath, string fileId)
        {
            try
            {
                string extentions = "";
                if (fileName.Trim().Contains("."))
                {
                    extentions = fileName.Split(".")[1];
                }
                string attachPath = SHLAPI.Utilities.StringUtil.SystemAttachmentsFolderPath + "\\" + innerPath + "\\" + fileId + "." + extentions;
                File.Delete(attachPath);
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public static string GetStringArabicVariants(string str, string fieldName)
        {
            if (str == null)
            {
                str = " 1=2 ";
                return str;
            }
            str = str.Trim();
            string temp = "";
            List<string> Strings = SetArabicVariantsWithOutSpace(str);
            if (Strings.Count != 1)
            {
                for (int i = 0; i < Strings.Count; i++)
                {
                    if (Strings[i].Trim() != "")
                    {
                        if (i == 0)
                            temp += " ( " + fieldName + " = N'" + Strings[i] + "'"; // //MLHIDE
                        else if (i == Strings.Count - 1)
                            temp += " OR " + fieldName + " = N'" + Strings[i] + "')"; // //MLHIDE
                        else
                            temp += " OR " + fieldName + " = N'" + Strings[i] + "'"; // //MLHIDE
                    }
                    else
                        temp += ") AND( 1=2 ";                    // //MLHIDE
                }
            }
            else
            {
                if (Strings[0].Trim() != "")
                    temp += fieldName + " = N'" + Strings[0] + "'"; // //MLHIDE
                else
                    temp = " 1=1 ";                               // //MLHIDE
            }
            return temp;
        }
        public static List<string> SetArabicVariants(string str)
        {
            string[] ArrOfAccount_Name = str.Trim().Split(' ');
            List<string> Strings = new List<string>();
            for (int i = 0; i < ArrOfAccount_Name.Length; i++)
            {
                Strings.Add(ArrOfAccount_Name[i]);
                if (ArrOfAccount_Name[i].Contains("ا"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('ا', 'أ'));
                }
                if (ArrOfAccount_Name[i].Contains("أ"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('أ', 'ا'));
                }
                if (ArrOfAccount_Name[i].Contains("ة") ||
                 ArrOfAccount_Name[i].Contains("ه"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('ة', 'ه'));
                    Strings.Add(ArrOfAccount_Name[i].Replace('ه', 'ة'));
                }
                if (i + 1 != ArrOfAccount_Name.Length)
                    Strings.Add("");
            }
            return Strings;
        }
        public static List<string> SetArabicVariantsWithOutSpace(string str)
        {
            List<string> Strings = new List<string>();
            Strings.Add(str);
            if (str.Contains("ا"))           // //MLHIDE
            {
                Strings.Add(str.Replace('ا', 'أ'));
                Strings.Add(str.Replace('ا', 'إ'));
                Strings.Add(str.Replace('ا', 'ى'));
            }
            if (str.Contains("ة"))           // //MLHIDE
            {
                Strings.Add(str.Replace('ة', 'ه'));
            }
            return Strings;
        }
        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        static public int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;
            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;
            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;
            if (targetWordCount == 0)
                return sourceWordCount;
            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];
            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;
            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }
        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        static public double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;
            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}