using System.Data;
using Dapper;
namespace SHLAPI.Models.Branches
{
    public class Branches_M
    {
        public int id { get; set; }
        public string no { get; set; }
        public string name { get; set; }
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, string code,int bankId)
        {
            try
            {
                string where = string.Format(" 1=1 and no='{0}' ", code);
                if (code == null || string.IsNullOrEmpty(code))
                {
                    where = " 1=1  ";
                }
                where += string.Format(" and parent_bank_id={0} order by id asc",bankId);
                string spName = "sp_Bank_branches_GetAllByWhere";
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
   
   
    }
}