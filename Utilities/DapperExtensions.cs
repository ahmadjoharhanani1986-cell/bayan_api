using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SHLAPI.Utilities
{
    public static class DapperExtensions
    {
        public static async Task<int?> InsertDynamic(this IDbConnection connection, string tableName, object obj,
        IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, object objSql = null)
        {
            var propNames = TypeDescriptor.GetProperties(obj).Select<PropertyDescriptor, string>(p => p.Name);

            var columns = string.Join(",", propNames);
            var values = string.Join(",", propNames.Select(name => "@" + name));

            if (objSql != null)
            {
                var sqlprops = TypeDescriptor.GetProperties(objSql);
                var sqlColumns = string.Join(",", sqlprops.Select<PropertyDescriptor, string>(p => p.Name));
                var sqlValues = string.Join(",", sqlprops.Select<PropertyDescriptor, string>(p => $"({p.GetValue(objSql)})"));
                if (sqlColumns.Length > 0)
                {
                    columns += "," + sqlColumns;
                    values += "," + sqlValues;
                }
            }

            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values}); SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await connection.QuerySingleAsync<int?>(sql, obj, transaction, commandTimeout, commandType);
        }

        public static async Task<int?> UpdateDynamic(this IDbConnection connection, string tableName, object obj,
        IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, string where = null, object objSql = null)
        {
            var props = TypeDescriptor.GetProperties(obj);
            var fields = string.Join(",", props.Select<PropertyDescriptor, string>(p => $"{p.Name} = @{p.Name}"));

            if (objSql != null)
            {
                var sqlprops = TypeDescriptor.GetProperties(objSql);
                var sqlFields = string.Join(",", sqlprops.Select<PropertyDescriptor, string>(p => $"{p.Name} = ({p.GetValue(objSql)})"));
                if (sqlFields.Length > 0)
                {
                    fields += "," + sqlFields;
                }
            }

            var sql = $"UPDATE {tableName} SET {fields}";
            if (where != null)
            {
                if (!where.StartsWith("WHERE", true, CultureInfo.InvariantCulture))
                    where = " WHERE " + where;
                sql += where;
            }
            return await connection.ExecuteAsync(sql, obj, transaction, commandTimeout, commandType);
        }
    }
}