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
        // Base SQL
        string sql = @"
            SELECT DISTINCT 
                V.id AS voucher_id,
                V.no AS voucher_no,
                V.person_account_id AS receiverpayer_id,
                V.person_name AS receiverpayer_name,
                C.id AS depitcredit_id,
                C.name AS depitcredit_name,
                DATE_FORMAT(V.date, '%Y-%m-%d') AS date,
                V.payment_type_id AS payment_id,
                PT.name AS payment_type,
                V.total_amount AS payment_amount,
                V.curr_id AS currency_id,
                CUR.name AS currency_name,
                V.manual_voucher_no AS manual_journal,
                V.cash_amount AS cach,
                V.check_amount AS `check`,
                V.status,
                V.type AS voucher_type
            FROM Vouchers_And_Bills V
            LEFT JOIN ChartOfAccount C ON C.id = V.person_account_id
            LEFT JOIN Payment_Types PT ON V.payment_type_id = PT.id
            LEFT JOIN Currency CUR ON V.curr_id = CUR.id
            LEFT JOIN vouchers_items_and_services VIS ON VIS.voucher_id = V.id
            WHERE V.type = @VoucherType
              AND V.status = 0
        ";

        // Dynamic filter
        var parameters = new DynamicParameters();
        parameters.Add("VoucherType", voucherType);

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            filterText = $"%{filterText.Trim()}%";
            sql += @"
              AND (
                  V.person_name LIKE @Filter
                  OR V.account_print_name LIKE @Filter
                  OR V.no LIKE @Filter
              )
            ";
            parameters.Add("Filter", filterText);
        }

        sql += " ORDER BY V.id DESC";

        // Execute query
        var res = await db.QueryAsync<dynamic>(sql, parameters, trans);
        return res;
    }
    catch (Exception ex)
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