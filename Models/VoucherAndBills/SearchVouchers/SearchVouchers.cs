using System.Data;
using Dapper;
using SHLAPI.Features;
using SHLAPI.Utilities;
using System.Text;
using SHLAPI.Features.Accounts;

namespace SHLAPI.Models.SearchVouchers
{
    public class SearchVouchers_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, string voucherType, string filterText)
        {
            try
            {
                string where = string.Format(" ( (Vouchers_And_Bills.type='{0}' ))", voucherType);
                if (filterText != null && filterText.Trim().Length > 0)
                {
                    string str = " and (" + Search_Specific(filterText.Trim(), "Vouchers_And_Bills.person_name") + "";    
                    where += str;
                    string account_print_name = " or "+ Search_Specific(filterText.Trim(), "Vouchers_And_Bills.account_print_name"); 
                    where += account_print_name;  
                     string code = " or "+ Search_Specific(filterText.Trim(), "Vouchers_And_Bills.no")+")"; 
                      where += code; 
                }

                where += " AND Vouchers_And_Bills.status=0 order by Vouchers_And_Bills.id desc ";
                string spName = "SelectVouchers_ByWhere_sp";
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
        



        public static string Search_Specific(string str, string fieldName)
        {
            try
            {

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
            catch (Exception EX)
            {
                return "";
            }
        }

        public static List<string> SetArabicVariants(string str)
        {
            try
            {
                string[] ArrOfAccount_Name = str.Trim().Split(' ');
                List<string> Strings = new List<string>();
                for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                {
                    Strings.Add(ArrOfAccount_Name[i]);
                    if (ArrOfAccount_Name[i].Contains("ا"))           // //MLHIDE
                    {
                        // ArrOfAccount_Name[i] = ArrOfAccount_Name[i] + " " + ArrOfAccount_Name[i].Replace('ا', 'أ');
                        Strings.Add(ArrOfAccount_Name[i].Replace('ا', 'أ'));
                        Strings.Add(ArrOfAccount_Name[i].Replace('ا', 'إ'));
                        Strings.Add(ArrOfAccount_Name[i].Replace('ا', 'ى'));
                    }
                    if (ArrOfAccount_Name[i].Contains("ة"))           // //MLHIDE
                    {
                        Strings.Add(ArrOfAccount_Name[i].Replace('ة', 'ه'));
                    }
                    if (i + 1 != ArrOfAccount_Name.Length)
                        Strings.Add("");
                }
                return Strings;
            }
            catch (Exception EX)
            {
                return null;
            }
        }

    }
}