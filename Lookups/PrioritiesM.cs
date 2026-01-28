using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SHLAPI.Features;

namespace SHLAPI.Models
{
    public class PrioritiesM
    {
        public int id { get; set; }

        public string name { get; set; }

        public int status { get; set; }

        public string icon_name { get; set; }
    
        public static async Task<Result> Save(IDbConnection db, IDbTransaction trans,string tableName, PrioritiesM obj)
        {
            Result result = new Result();
            string cmd = "";
            if (obj.id == 0)
            {
                cmd =string.Format("INSERT INTO priority_tbl (name,status,icon_name) values(@name,1,@icon_name)");
            }
            else
            {
                cmd = string.Format("UPDATE priority_tbl set name=@name,status=@status,icon_name WHERE id=@id");
            }
            await db.ExecuteAsync(cmd, obj, trans);
            if (obj.id == 0)
            {
                obj.id = await CommonM.GetLastId(db, trans);
            }
            return result;
        }


        public static async Task<Result> Delete(IDbConnection db, IDbTransaction trans,string tableName, int id)
        {
            return await CommonM.DeleteRow(db,trans,tableName,id);
        }

        public static async Task<LookupM> Get(IDbConnection db, IDbTransaction trans,string tableName,int id, NavigationTypes nav)
        {
            string query = string.Format("SELECT id,name,status from priority_tbl where (status=1 or status=2)");
            query = CommonM.AddNavigationQuery(query, nav);

            var result = (await db.QueryAsync<LookupM>(query, new { id = id })).FirstOrDefault();
            return result;
        }

        public static async Task<IEnumerable<PrioritiesM>> GetList(IDbConnection db, IDbTransaction trans,int? status,int? id)
        {
            string query = string.Format("SELECT id,name,status,icon_name from priority_tbl where status!=99");
            if(status!=null && status>0)
            {
                query+=" and  status=@status";
            }

            if(id!=null && id>0)
            {
                query+=" and  id=@id";
            }

            query += " order by id desc";

            var result = await db.QueryAsync<PrioritiesM>(query,new {status=status,id=id});
            return result;
        }

        public static async Task<Result> ChangeStatus(IDbConnection db, IDbTransaction trans,string tableName, int id, int status)
        {
            Result result = new Result();
            string query = "";
            query = string.Format("update priority_tbl set");
            query += " status=@status";
            query += " where id=@id";
            await db.ExecuteAsync(query, new { id = id, status = status }, trans);
            return result;
        }

    }
}
