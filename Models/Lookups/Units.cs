using System.Data;
using Dapper;

namespace SHLAPI.Models.Lookups
{
    public class Unit_M
    {
        public int id { get; set; }
        public string name { get; set; }
        public string state { get; set; }

        // 🔹 Lookup Get All
        public static async Task<IEnumerable<dynamic>> GetAll(IDbConnection db, IDbTransaction trans,int id,string state)
        {
            string sql = @"
                            SELECT id, name, state
                            FROM Units
                            WHERE 1=1 ";
                            if(state !="-1")
                             sql +=" and state = @state";
                            if(id >0)
                             sql += @" and  id = @id ";

            return await db.QueryAsync<dynamic>(
                sql,
                new
                {
                    id,
                    state
                },
                transaction: trans
            );
        }
    }
}
