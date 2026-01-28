using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SHLAPI.Features;

namespace SHLAPI.Models
{
    public class CustomersM
    {
        public int id { get; set; }

        public string name { get; set; }

        public int status { get; set; }

        public static async Task<IEnumerable<IdNameDTO>> GetListIdName(IDbConnection db, IDbTransaction trans)
        {
            string query = @"SELECT id
                                    ,name
                                    ,status
                                    FROM customers_tbl
                                    where status!=99
                                    ";

            CommonM.AdjustQuery(query);
            var result = await db.QueryAsync<IdNameDTO>(query);
            return result;
        }

    }
}
