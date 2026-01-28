using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System;


namespace SHLAPI.Models.LogFile
{
    public class LogTrialM
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int user_id { get; set; }
        //  operation_id ==> //مشاهدة8  //7 تحويل لاكسل // 6 طباعة//5 حذف//4 تفعيل//3 تجميد //2 تعديل// 1 ادخال //
        public int operation_id { get; set; }
        public int page_id { get; set; }
        public int lang_id { get; set; }
        public int record_id { get; set; }
        public DateTime date_ { get; set; }
        public DateTime time_ { get; set; }
        public string note { get; set; }
        public byte[] object_ { get; set; }
        public int log_file_id { get; set; }
        public static async Task<CommonM.ModelResult> Save(IDbConnection db, IDbTransaction trans, LogTrialM auditTrailMObj)
        {
            CommonM.ModelResult modelResultObj = new CommonM.ModelResult();
            modelResultObj.result = false;
            modelResultObj.lastId = 0;
            string query = "insert into logfile_tbl(company_id,user_id,operation_id,page_id,record_id,date_,time_,note) ";
            query += " values(@company_id,@user_id,@operation_id,@page_id,@record_id,@date_,@time_,@note)";
            await db.ExecuteAsync(query, auditTrailMObj, trans);
            modelResultObj.lastId = await CommonM.GetLastId(db, trans);
            modelResultObj.result = true;
            return modelResultObj;
        }
    }

}