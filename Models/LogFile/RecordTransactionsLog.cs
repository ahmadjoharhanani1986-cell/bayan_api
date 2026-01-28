using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System;

using SHLAPI.Features.LogFile;
using System.ComponentModel.DataAnnotations.Schema;

namespace SHLAPI.Models.LogFile
{
    public class RecordTransactionsLogM
    {
        public int ser { get; set; }
        public string userName { get; set; }
        public string operationName { get; set; }
        public string date_ { get; set; }
        public string time_ { get; set; }
        public string note { get; set; }
        public static async Task<IEnumerable<RecordTransactionsLogM>> GetRecordTransactionsLog(IDbConnection db, GetRecordTransactionsLogF.Query query)
        {
            string selectStatment = "select ROW_NUMBER() OVER (ORDER BY logfile_tbl.record_id,logfile_tbl.date_,logfile_tbl.time_) ser, users_tbl.name as userName,";
            selectStatment += " operation_captions_tbl.name as operationName,CONVERT(logfile_tbl.date_,CHAR) as date_,CONVERT(logfile_tbl.time_, CHAR) as time_,logfile_tbl.note ";
            selectStatment += " from logfile_tbl";
            selectStatment += " inner join operations_tbl on operations_tbl.id = logfile_tbl.operation_id";
            selectStatment += " inner join operation_captions_tbl on operation_captions_tbl.operation_id = operations_tbl.id and operation_captions_tbl.language_id =@language_id";
            selectStatment += " inner join users_tbl on users_tbl.id = logfile_tbl.user_id";
            selectStatment += " where record_id =@record_id and page_id =@page_id and users_tbl.company_id = @company_id";
            
            var res = await db.QueryAsync<RecordTransactionsLogM>(selectStatment, query);
            return res;
        }
    }

}