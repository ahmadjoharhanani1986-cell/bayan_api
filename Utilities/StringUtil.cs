using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SHLAPI.Utilities
{
    public static class StringUtil
    {
        public static int AccountCodeLength = 9;
        public static int ItemCodeLength = 9;
        public static int selectTopCount = 5;
        public static int selectTopCounForItems = 100;
        public static int SalesManFatherId = 6;
        public static int CustomerSuppliersFatherId = 147;
        public static int MainCurrId = 4;
        public static int SystemAttachmentsSaveOption = 2;// 1 DB  2 File
        public static string SystemAttachmentsFolderPath = "C:\\";
        public static string RemoveAllButLettersAndNumbers(this string input)
        {
            char[] arr = input.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c))).ToArray();

            return new string(arr);
        }
public static float MillimetersToPoints(float mm)
    {
        return mm * 72 / 25.4f;
    }
        public static string SubstringISCO(this string input,int length)
        {
            string str=input;
            if(str.Length>length)
                input=input.Substring(length);
            return str;
        }

        public static int TryParseInt(this string input, int defaultValue = 0)
        {
            int x = defaultValue;
            int.TryParse(input, out x);
            return x;
        }

        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string Sha512Hash(this string input)
        {
            var sha512 = new SHA512Managed();
            sha512.Initialize();
            var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hashedPasword = Encoding.UTF8.GetString(hash);
            return hashedPasword;
        }

        public static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        public static string ToString(this Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public static string AdjsetCodeCostCenter(string code, int type_id)
        {
            string returnedCode = type_id.ToString();
            if (code == "")
            {
                for (int i = 0; i < AccountCodeLength.ToString().ToArray().Length; i++)
                    code += "0";
                int maxCode = 1;
                string codeWithOutType = code.Replace(type_id.ToString(), "");

                int.TryParse(codeWithOutType, out maxCode);
                ++maxCode;
                char[] arrCodeChar = code.ToCharArray();
                for (int i = returnedCode.ToCharArray().Length; i < (8 - maxCode.ToString().ToCharArray().Length); i++)
                {
                    returnedCode += "0";
                }
                returnedCode += maxCode;
            }
            else
            {
                returnedCode = Inc_Code(code);
            }
            return returnedCode;
        }
        public static string AdjsetCodeAccountNo(string code, string prefix)
        {
            string returnedCode = "";
            if (code == "")
            {
                for (int i = 0; i < AccountCodeLength; i++)
                    code += "0";
                int maxCode = 1;
                string codeWithOutType = code;
                if (prefix != "")
                    codeWithOutType = code.Replace(prefix, "");

                int.TryParse(codeWithOutType, out maxCode);
                ++maxCode;
                char[] arrCodeChar = code.ToCharArray();
                for (int i = prefix.ToCharArray().Length; i < (Utilities.StringUtil.AccountCodeLength - maxCode.ToString().ToCharArray().Length); i++)
                {
                    returnedCode += "0";
                }
                returnedCode = prefix + returnedCode;
                returnedCode += maxCode;
            }
            else
            {
                returnedCode = prefix + Inc_CodeAccount(code, prefix);
            }
            return returnedCode;
        }
        public static string AdjsetItemServiceCode(string code)
        {
            // متكرر  4AB000010 غير متكرر ويتم إعادة الرقم الالي الجديد للفرونت اند لعرضه في اليرت (ليس مربع رسالة) 
            string returnedCode = "";
            string prefix = "";
            for (int y = code.Length - 1; y >= 0; y--)
            {
                if (!char.IsDigit(code.ToCharArray()[y]))
                {
                    prefix += code.Substring(0, y + 1);
                    break;
                }
            }

            if (code == "")
            {
                for (int i = 0; i < AccountCodeLength; i++)
                    code += "0";
                int maxCode = 1;
                string codeWithOutType = code;
                if (prefix != "")
                    codeWithOutType = code.Replace(prefix, "");

                int.TryParse(codeWithOutType, out maxCode);
                ++maxCode;
                char[] arrCodeChar = code.ToCharArray();
                for (int i = prefix.ToCharArray().Length; i < (Utilities.StringUtil.AccountCodeLength - maxCode.ToString().ToCharArray().Length); i++)
                {
                    returnedCode += "0";
                }
                returnedCode = prefix + returnedCode;
                returnedCode += maxCode;
            }
            else
            {
                returnedCode = prefix + Inc_CodeAccount(code, prefix);
            }
            return returnedCode;
        }
        static public string GetFillterSearchAccount(string tableName, int lang_id, int filterMode, string filterText)
        {
            string _fillter = "";
            char[] sep = { ' ' };
            if (filterText == null || filterText.Trim() == "") return "";
            string[] ArrOfAccount_Name = filterText.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (ArrOfAccount_Name.Length != 0)
            {
                _fillter += " and ( 1=1 ";
                for (int i = 0; i < ArrOfAccount_Name.Length; i++)
                {
                    if (ArrOfAccount_Name[i].Trim() == "")
                    {
                        continue;
                    }
                    string columnName = tableName + ".name";
                    if (lang_id != 1) columnName = tableName + ".name_lang2";
                    if (filterMode == 1)// يحتوي على
                        _fillter += "  and " + columnName + " like N'%" + ArrOfAccount_Name[i] + "%'";
                    else if (filterMode == 2)//  يبدأ ب
                        _fillter += "  and " + columnName + " like N'" + ArrOfAccount_Name[i] + "%'";
                    else if (filterMode == 3)// ينتهي  ب
                        _fillter += "  and " + columnName + " like N'%" + ArrOfAccount_Name[i] + "'";
                }
                _fillter += " ) ";
            }
            _fillter += " or " + tableName + ".code='" + filterText + "'";
            return _fillter;
        }
        static public string Inc_CodeAccount(string code, string prefix)
        {
            if (code.Trim().Length == 0) return code;
            string codeValue = prefix;
            if (code.Length > 0)
            {
                code = code.Remove(0, prefix.Length);
            }
            int i;
            if (code == "")
            {
                code = codeValue + "00000000";                        // //MLHIDE
            }
            char[] code2 = code.ToCharArray();
            i = code2.Length - 1;
            while (i > 0 && code2[i] == 32) i--;
            if (code2[i] == '9')
            {
                while (code2[i] == '9' && i > 0)
                {
                    code2[i] = '0';
                    i--;
                }
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            else
            {
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            string newCode = new string(code2);
            return newCode;

        }// Incerement Customer Code...
        static public string Inc_Code(string code)
        {
            if (code.Trim().Length == 0) return code;
            char codeValue = code[0];
            if (code.Length > 0)
            {
                if (code[0] == 'R') code = code.Remove(0, 1);
                else if (code[0] == 'J') code = code.Remove(0, 1);
                else if (code[0] == 'P') code = code.Remove(0, 1);
                else if (code[0] == 'C') code = code.Remove(0, 1);
                else if (code[0] == 'D') code = code.Remove(0, 1);
                else if (code[0] == 'E') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'I') code = code.Remove(0, 1);
                else if (code[0] == 'A') code = code.Remove(0, 1);
                else if (code[0] == 'H') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'M') code = code.Remove(0, 1);
                else if (code[0] == 'B') code = code.Remove(0, 1);
                else if (code[0] == 'V') code = code.Remove(0, 1);
                else if (code[0] == 'T') code = code.Remove(0, 1);
            }
            int i;
            if (code == "")
            {
                code = codeValue + "00000000";                        // //MLHIDE
            }
            char[] code2 = code.ToCharArray();
            i = code2.Length - 1;
            while (i > 0 && code2[i] == 32) i--;
            if (code2[i] == '9')
            {
                while (code2[i] == '9' && i > 0)
                {
                    code2[i] = '0';
                    i--;
                }
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            else
            {
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]++;
            }
            string newCode = new string(code2);
            return newCode;

        }// Incerement Customer Code...
        static public string Dec_Code(string code)
        {
            char codeValue = code[0];
            if (code.Length > 0)
            {
                if (code[0] == 'R') code = code.Remove(0, 1);
                else if (code[0] == 'J') code = code.Remove(0, 1);
                else if (code[0] == 'P') code = code.Remove(0, 1);
                else if (code[0] == 'C') code = code.Remove(0, 1);
                else if (code[0] == 'D') code = code.Remove(0, 1);
                else if (code[0] == 'E') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'I') code = code.Remove(0, 1);
                else if (code[0] == 'A') code = code.Remove(0, 1);
                else if (code[0] == 'H') code = code.Remove(0, 1);
                else if (code[0] == 'S') code = code.Remove(0, 1);
                else if (code[0] == 'M') code = code.Remove(0, 1);
                else if (code[0] == 'B') code = code.Remove(0, 1);
                else if (code[0] == 'V') code = code.Remove(0, 1);
                else if (code[0] == 'T') code = code.Remove(0, 1);
            }
            int i;
            if (code == "")
            {
                code = codeValue + "00000000";                        // //MLHIDE
            }
            char[] code2 = code.ToCharArray();
            i = code2.Length - 1;
            while (i > 0 && code2[i] == 32) i--;
            if (code2[i] == '9')
            {
                while (code2[i] == '9' && i > 0)
                {
                    code2[i] = '0';
                    i--;
                }
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]--;
            }
            else
            {
                if (code2[i] == '9') code2[i] = 'A';
                else code2[i]--;
            }
            string newCode = new string(code2);
            return newCode;

        }// Decrement Customer Code...
        public static string GetNavigationResultSelectStatment(string tableName, int navValue, int companyId, int id, int classificationTypeId = 0, int type_id = 0, int cust_type = 0,
                                                                bool withStatus=true)
        {
            string selectNavigation = "";
            switch (navValue)
            {
                case 1://navigation_enum.first
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                        if(withStatus)
                        selectNavigation += " and status <> 3  ";
                        if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                        if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                        if (type_id > 0) selectNavigation += " and type_id= @type_id";
                        if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                        selectNavigation += " order by id asc LIMIT 1 ";
                        break;
                    }
                case 2: //navigation_enum.last
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                          if(withStatus)
                        selectNavigation += " and status <> 3  ";
                        if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                        if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                        if (type_id > 0) selectNavigation += " and type_id= @type_id";
                        if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                        selectNavigation += " order by id desc LIMIT 1 ";
                        break;
                    }
                case 3://navigation_enum.next
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                              if(withStatus)
                            selectNavigation += " and status <> 3  ";
                            if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                            if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                            if (type_id > 0) selectNavigation += " and type_id= @type_id";
                            if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                            selectNavigation += " order by id desc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                              if(withStatus)
                            selectNavigation += " and status <> 3  ";
                            if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                            selectNavigation += " and id > " + id + "";
                            if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                            if (type_id > 0) selectNavigation += " and type_id= @type_id";
                            if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                            selectNavigation += " order by id asc LIMIT 1 ";
                        }
                        break;
                    }
                case 4://navigation_enum.prev
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                              if(withStatus)
                            selectNavigation += " and status <> 3  ";
                            if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                            if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                            if (type_id > 0) selectNavigation += " and type_id= @type_id";
                            if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                            selectNavigation += "  order by id asc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where 1=1 ";
                              if(withStatus)
                            selectNavigation += " and status <> 3  ";
                            if (companyId > 0) selectNavigation += " and company_id = " + companyId;
                            selectNavigation += " and id < " + id + "";
                            if (classificationTypeId > 0) selectNavigation += " and classification_type_id= " + classificationTypeId;
                            if (type_id > 0) selectNavigation += " and type_id= @type_id";
                            if (cust_type > 0) selectNavigation += " and cust_type= @cust_type";
                            selectNavigation += " order by id desc LIMIT 1 ";
                        }
                        break;
                    }
                default: { break; }
            }
            return selectNavigation;
        }
        public static string GetItemAndServiceNavigationResultSelectStatment(string tableName, int navValue, int companyId, int id, int type)
        {
            string selectNavigation = "";
            switch (navValue)
            {
                case 1://navigation_enum.first
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                        if (type > 0) selectNavigation += " and type= @type";
                        selectNavigation += " order by id asc LIMIT 1 ";
                        break;
                    }
                case 2: //navigation_enum.last
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                        if (type > 0) selectNavigation += " and type= @type";
                        selectNavigation += " order by id desc LIMIT 1 ";
                        break;
                    }
                case 3://navigation_enum.next
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                            if (type > 0) selectNavigation += " and type= @type";
                            selectNavigation += " order by id desc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and id >" + id + " and status <> 3";
                            if (type > 0) selectNavigation += " and type= @type";
                            selectNavigation += " order by id asc LIMIT 1 ";
                        }
                        break;
                    }
                case 4://navigation_enum.prev
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                            if (type > 0) selectNavigation += " and type= @type";
                            selectNavigation += "  order by id asc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + "  and id < " + id + " and status <> 3";
                            if (type > 0) selectNavigation += " and type= @type";
                            selectNavigation += " order by id desc LIMIT 1 ";
                        }
                        break;
                    }
                default: { break; }
            }
            return selectNavigation;
        }

        public static string GetNavigationResultSelectStatmentByTree(string tableName, string Column1InWhere, int navValue, int companyId, int id, int typeId = 0, int maxTypeId = 0, string orderColumnName = "id")
        {
            string selectNavigation = "";
            switch (navValue)
            {
                case 1://navigation_enum.first
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3  ";
                        if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                        selectNavigation += " order by " + Column1InWhere + "," + orderColumnName + " asc LIMIT 1 ";
                        break;
                    }
                case 2: //navigation_enum.last
                    {
                        selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                        if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                        selectNavigation += " order by " + Column1InWhere + "," + orderColumnName + " desc LIMIT 1 ";
                        break;
                    }
                case 3://navigation_enum.next
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                            if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                            if (maxTypeId > 0) selectNavigation += " and " + Column1InWhere + "> " + maxTypeId;
                            selectNavigation += " order by " + Column1InWhere + "," + orderColumnName + " desc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3  and id >" + id + "";
                            if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                            selectNavigation += " order by " + Column1InWhere + "," + orderColumnName + " asc LIMIT 1 ";
                        }
                        break;
                    }
                case 4://navigation_enum.prev
                    {
                        if (id == 0)
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3";
                            if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                            if (maxTypeId > 0)
                            {
                                selectNavigation += " and " + Column1InWhere + " < " + maxTypeId;
                                selectNavigation += "  order by " + Column1InWhere + "," + orderColumnName + " desc LIMIT 1 ";
                            }
                            else selectNavigation += "  order by " + Column1InWhere + "," + orderColumnName + " asc LIMIT 1 ";
                        }
                        else
                        {
                            selectNavigation = " SELECT id FROM " + tableName + " where company_id=" + companyId + " and status <> 3  and id < " + id + "";
                            if (typeId > 0) selectNavigation += " and " + Column1InWhere + "= " + typeId;
                            selectNavigation += " order by " + Column1InWhere + "," + orderColumnName + " desc LIMIT 1 ";
                        }
                        break;
                    }
                default: { break; }
            }
            return selectNavigation;
        }
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static string Decrypt(string encryptedText)
        {
            string plaintext = "";
            byte[] cipherText = StringToByteArray(encryptedText);//"1cad11147e00fa2f0524bc211d2d79a8");

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                byte[] Key = new byte[16];
                Key[0] = (byte)1;
                Key[1] = (byte)2;
                Key[2] = (byte)3;
                Key[3] = (byte)4;
                Key[4] = (byte)5;
                Key[5] = (byte)6;
                Key[6] = (byte)7;
                Key[7] = (byte)8;
                Key[8] = (byte)9;
                Key[9] = (byte)10;
                Key[10] = (byte)11;
                Key[11] = (byte)12;
                Key[12] = (byte)13;
                Key[13] = (byte)14;
                Key[14] = (byte)15;
                Key[15] = (byte)16;
                byte[] IV = new byte[16];
                IV[0] = (byte)21;
                IV[1] = (byte)22;
                IV[2] = (byte)23;
                IV[3] = (byte)24;
                IV[4] = (byte)25;
                IV[5] = (byte)26;
                IV[6] = (byte)27;
                IV[7] = (byte)28;
                IV[8] = (byte)29;
                IV[9] = (byte)30;
                IV[10] = (byte)31;
                IV[11] = (byte)32;
                IV[12] = (byte)33;
                IV[13] = (byte)34;
                IV[14] = (byte)35;
                IV[15] = (byte)36;
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Padding = PaddingMode.None;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                            if (!string.IsNullOrEmpty(plaintext) && plaintext.Contains("|"))
                            {
                                //plaintext = plaintext.Split('|')[0];
                                plaintext = plaintext.Substring(0, plaintext.LastIndexOf("|"));

                            }
                        }
                    }
                }
            }
            return plaintext;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return StringToByteArrayFastest(hex);
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        ﻿ public static string Search_Specific(string str, string fieldName)
        {
            try
            {

                str = str.Trim();
                //if (str != "")
                //{
                //    str = str.Replace('أ', 'ا');
                //    str = str.Replace('إ', 'ا');
                //    //str = str.Replace('ى', 'ا');
                //    str = str.Replace('ه', 'ة');
                //}
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