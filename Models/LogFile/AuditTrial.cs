using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System;


namespace SHLAPI.Models.LogFile
{
    public class AuditTrailM
    {
        public int id { get; set; }
        public int company_id {get;set;}
        public int user_id {get;set;}
        //  operation_id ==> //مشاهدة8  //7 تحويل لاكسل // 6 طباعة//5 حذف//4 تفعيل//3 تجميد //2 تعديل// 1 ادخال //
        public int operation_id {get;set;}
        public int page_id {get;set;}
        public int record_id {get;set;}
        public DateTime date_ { get; set; }
        public DateTime time_ { get; set; }
        public string note { get; set; }   
        public byte [] object_ {get;set;}
        public int log_file_id {get;set;}
        public static async Task<CommonM.ModelResult> Save(IDbConnection db,IDbTransaction trans,AuditTrailM auditTrailMObj)
        {
            CommonM.ModelResult modelResultObj = new CommonM.ModelResult();
            modelResultObj.result = false;
            modelResultObj.lastId =0;

            auditTrailMObj.note = auditTrailMObj.note.Substring(0,auditTrailMObj.note.Length>199?199:auditTrailMObj.note.Length);

            int logFileId =0;
             string  query ="insert into logfile_tbl(user_id,operation_id,page_id,record_id,date_,time_,note) ";
                     query += " values(@user_id,@operation_id,@page_id,@record_id,@date_,@time_,@note)";  
             await db.ExecuteAsync(query, auditTrailMObj,trans);

             logFileId= await CommonM.GetLastId(db,trans);
             auditTrailMObj.log_file_id = logFileId;
             query ="insert into audit_trail_tbl(log_file_id,object_) ";
                    query += " values(@log_file_id,@object_)";  
             await db.ExecuteAsync(query, auditTrailMObj,trans);
             modelResultObj.lastId = await CommonM.GetLastId(db,trans);
             modelResultObj.result =true;        
             return modelResultObj;        
        } 
    }

}