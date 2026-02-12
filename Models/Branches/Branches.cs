using System.Data;
using Dapper;
namespace SHLAPI.Models.Branches
{
    public class Branches_M
    {
        public int id { get; set; }
        public string no { get; set; }
        public string name { get; set; }
public static async Task<IEnumerable<dynamic>> GetData(
    IDbConnection db,
    IDbTransaction trans,
    string code,
    int bankId)
{
    try
    {
        string sql = @"
        SELECT 
            id,
            no,
            name,
            parent_bank_id,
            region_id,
            address,
            phone_no,
            fax_no,
            telefax,
            email,
            pobox,
            manager_name,
            manager_contact_info,
            note
        FROM Bank_branches
        WHERE 1=1";

        var param = new DynamicParameters();

        if (!string.IsNullOrEmpty(code))
        {
            sql += " AND no = @code ";
            param.Add("@code", code);
        }

        if (bankId > 0)
        {
            sql += " AND parent_bank_id = @bankId ";
            param.Add("@bankId", bankId);
        }

        sql += " ORDER BY id ASC;";

        var res = await db.QueryAsync<dynamic>(
            sql,
            param,
            transaction: trans,
            commandType: CommandType.Text
        );

        return res;
    }
    catch
    {
        throw;
    }
}

   
    }
}