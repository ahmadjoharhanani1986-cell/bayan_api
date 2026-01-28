using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using Dapper;
using SHLAPI.Features.InvoiceVoucher;
using System.Runtime.Serialization.Formatters.Binary;

namespace SHLAPI.Models.SearchItems
{
    public class SearchItems_M
    {
        public int id { get; set; }
        public string name { get; set; }
        public string no { get; set; }
        public int BarCodesCount { get; set; }
        public double price { get; set; }
        public string CurrencyName { get; set; }
        public string notes { get; set; }
        public string part_number { get; set; }
        public int image_id { get; set; }
        public byte[] image { get; set; }
        public string image64 { get; set; }
        public static async Task<IEnumerable<SearchItems_M>> GetData(IDbConnection db, IDbTransaction trans, GetSearchItemsF.Query obj)
        {
            try
            {
                string _fillterPublic = "";
                if (!obj._getSusspended)
                    _fillterPublic += "  and Suspended='false' ";
                string Account_Name = obj.filterText == null ? "" : obj.filterText;//.Trim();
                string s = obj.filterText;
                char[] sep = { ' ' };
                string[] ArrOfAccount_Name = Account_Name.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (ArrOfAccount_Name.Length != 0)
                {
                    _fillterPublic += " and ( 1=1 ";
                    if (Account_Name.Trim() != "")
                    {
                        _fillterPublic += " and ( 1=1 ";
                    }
                    for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                    {
                        if (ArrOfAccount_Name[i].Trim() == "")
                        {
                            continue;
                        }
                        _fillterPublic += "  and Replace(Items_and_services.name,'ه','ة')   like ";
                        _fillterPublic += "  Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') ";
                    }
                    _fillterPublic += " ) ";
                    if (obj.chkIncludeBarCodeWithSearch)
                    {
                        _fillterPublic += " OR ( 1=1 ";
                        for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                        {
                            if (ArrOfAccount_Name[i].Trim() == "")
                            {
                                continue;
                            }

                            _fillterPublic += "  Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') ";

                        }
                        _fillterPublic += " ) ";

                        _fillterPublic += " OR ( 1=1 ";
                        for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                        {
                            if (ArrOfAccount_Name[i].Trim() == "")
                            {
                                continue;
                            }

                            _fillterPublic += "  Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') ";
                        }
                        _fillterPublic += " ) ";

                        _fillterPublic += " OR ( 1=1 ";
                        for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                        {
                            if (ArrOfAccount_Name[i].Trim() == "")
                            {
                                continue;
                            }

                            _fillterPublic += "  Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') ";

                        }
                        _fillterPublic += " ) ";

                        if (obj.showNotes)
                        {
                            _fillterPublic += " OR ( 1=1 ";
                            for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                            {
                                if (ArrOfAccount_Name[i].Trim() == "")
                                {
                                    continue;
                                }

                                _fillterPublic += "  Replace(N'%" + ArrOfAccount_Name[i] + "%','ه','ة') ";

                            }
                            _fillterPublic += " ) ";
                        }

                    }

                    _fillterPublic += " ) ";

                }
                string Str_isDefault = "";
                if (!(obj.showCurrency || obj.showPrice || obj.showNotes))
                    Str_isDefault = "1";
                else
                    Str_isDefault = "0";


                string Str_PurchaseOrSell = "0";
                if (obj.type != "I")
                    Str_PurchaseOrSell = "1";





                string where = " and (dbo.Items_and_services.type_id = 1  ";
                if (!obj.dontGetService)
                    where += "  or ( dbo.Items_and_services.type_id=2  and dbo.Items_and_services.service_type='1')  ";
                where += ")";
                // if (filterText != null && filterText.Trim().Length > 0)
                // {
                //     string str = " and (" + Search_Specific(filterText.Trim(), "Items_and_services.name") + "";
                //     where += str;
                //     string code = " or " + Search_Specific(filterText.Trim(), "Items_and_services.no") + ")";
                //     where += code;
                // }
                where += _fillterPublic;

                string spName = "SeacrhByBarCode_Sp";
                var param = new
                {
                    @Str1 = where,
                    @Str2 = Str_PurchaseOrSell,
                    @Str3 = Str_isDefault
                };
                var res = await db.QueryAsync<SearchItems_M>(
                     spName,
                     param,
                     transaction: trans,
                    commandType: CommandType.StoredProcedure
                );
                if (obj.getItemImg)
                    if (res != null)
                    {
                        var list = res.ToList();

                        for (int i = 0; i < list.Count; i++)
                        {
                            var item = list[i];

                            // تحقق من وجود الخاصية image_id وعدم كونها null
                            if (item.image_id > 0)
                            {

                                string whereStat = " where id=" + item.image_id;
                                var sql = $"select attachment from Images {whereStat}";
                                item.image64 = await db.ExecuteScalarAsync<string>(sql, transaction: trans);
                                // item.image64 = Convert.ToBase64String(item.image);
                                item.image = null;
                            }
                            item.price = Math.Round(item.price, 2);
                        }
                    }
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