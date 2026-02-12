using System.Data;
using Dapper;
namespace SHLAPI.Models.Banks
{
    public class Banks_M
    {
        public int id { get; set; }
        public string no { get; set; }
        public string name { get; set; }
      public static async Task<IEnumerable<dynamic>> GetData(
    IDbConnection db, 
    IDbTransaction trans, 
    string code)
{
    try
    {
        string sql = @"SELECT id, no, name, web_site, notes 
                       FROM Banks 
                       WHERE 1=1 ";

        var param = new DynamicParameters();

        if (!string.IsNullOrEmpty(code))
        {
            sql += " AND no = @code ";
            param.Add("@code", code);
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