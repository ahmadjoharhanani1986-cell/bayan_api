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
    public class LookupM
    {
        private string tableName { get; set; }
        public LookupM(string _tableName)
        {
            tableName = _tableName;
        }

        private LookupM()
        {
        }

        public int id { get; set; }

        public int crm_id { get; set; }

        public string name { get; set; }

        public int status { get; set; }

        public static async Task<Result> Save(IDbConnection db, IDbTransaction trans, string tableName, LookupM obj)
        {
            Result result = new Result();
            string cmd = "";
            if (obj.id == 0)
            {
                cmd = string.Format("INSERT INTO {0} (name,status) values(@name,1)", tableName);
            }
            else
            {
                cmd = string.Format("UPDATE {0} set name=@name,status=@status WHERE id=@id", tableName);
            }
            await db.ExecuteAsync(cmd, obj, trans);
            if (obj.id == 0)
            {
                obj.id = await CommonM.GetLastId(db, trans);
            }
            return result;
        }


        public static async Task<Result> Delete(IDbConnection db, IDbTransaction trans, string tableName, int id)
        {
            return await CommonM.DeleteRow(db, trans, tableName, id);
        }

        public static async Task<LookupM> Get(IDbConnection db, IDbTransaction trans, string tableName, int id, NavigationTypes nav = NavigationTypes.GetIt)
        {
            string query = string.Format("SELECT id,name,status from {0} where (status=1 or status=2)", tableName);
            query = CommonM.AddNavigationQuery(query, nav);

            var result = (await db.QueryAsync<LookupM>(query, new { id = id })).FirstOrDefault();
            return result;
        }

        public static async Task<IEnumerable<LookupM>> GetList(IDbConnection db, IDbTransaction trans, string tableName, int? status, int? id)
        {
            string query = string.Format("SELECT id,name,status from {0} where status!=99", tableName);
            if (status != null && status > 0)
            {
                query += " and  status=@status";
            }

            if (id != null && id > 0)
            {
                query += " and  id=@id";
            }

            if (tableName == "tasks_types_statuses_tbl")
                query = string.Format("SELECT id,CONCAT(task_type_name,' ',name) as name,status from {0} where (status=1 or status=2)", tableName);

            if (tableName == "employees_tbl")
                query = string.Format("SELECT id,crm_emplyee_id AS crm_id,name,status from {0} where status!=99", tableName);

            query += " order by id asc";

            var result = await db.QueryAsync<LookupM>(query, new { status = status, id = id });
            return result;
        }

        public static async Task<Result> ChangeStatus(IDbConnection db, IDbTransaction trans, string tableName, int id, int status)
        {
            Result result = new Result();
            string query = "";
            query = string.Format("update {0} set", tableName);
            query += " status=@status";
            query += " where id=@id";
            await db.ExecuteAsync(query, new { id = id, status = status }, trans);
            return result;
        }

        public static async Task<Result> SaveEmployee(IDbConnection db, IDbTransaction trans, LookupM obj)
        {
            Result result = new Result();
            string cmd = "";
            if (obj.id == 0)
            {
                cmd = "INSERT INTO employees_tbl (name,status, crm_emplyee_id) values(@name,1,@crm_id)";
            }
            else
            {
                cmd = "UPDATE employees_tbl set name=@name,status=@status, crm_emplyee_id=@crm_id WHERE id=@id";
            }
            await db.ExecuteAsync(cmd, obj, trans);
            if (obj.id == 0)
            {
                obj.id = await CommonM.GetLastId(db, trans);
            }
            return result;
        }

        public static async Task<IEnumerable<EmployeeDTO>> GetEmployeesList(IDbConnection db, IDbTransaction trans, int? status, int? id)
        {
            string query = string.Format("SELECT id,crm_emplyee_id AS crm_id,name,status, ((SELECT COUNT(*) FROM default_release_employees WHERE employee_id = id) > 0) AS is_default from employees_tbl where status!=99");
            if (status != null && status > 0)
            {
                query += " and  status=@status";
            }

            if (id != null && id > 0)
            {
                query += " and  id=@id";
            }

            query += " order by id asc";

            var result = await db.QueryAsync<EmployeeDTO>(query, new { status = status, id = id });
            return result;
        }

    }
}
