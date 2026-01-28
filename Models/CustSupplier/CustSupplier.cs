using System.Data;
using Dapper;
namespace SHLAPI.Models.CustSupplier
{
    public class CustSupplier_M
    {
        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int coaId)
        {
            try
            {
                string sql = @"SELECT *
               FROM Custs_Suppliers
               WHERE coa_id = @coaId";
                var result = await db.QueryAsync<dynamic>(sql, new { coaId },trans);
                return result;
            }
            catch (Exception EX)
            {
                throw;
            }
        }


    }
}