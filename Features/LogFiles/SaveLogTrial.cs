using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using FluentValidation;
using Mapster;
using MediatR;
using SHLAPI.Models;

namespace SHLAPI.Features.LogFile
{
    public class SaveLogTrialF
    {
        public class Command : IRequest<Result>
        {
         public int id { get; set; }
        public int company_id {get;set;}
        public int user_id {get;set;}
        //  operation_id ==> //مشاهدة8  //7 تحويل لاكسل // 6 طباعة//5 حذف//4 تفعيل//3 تجميد //2 تعديل// 1 ادخال //
        public int operation_id {get;set;}
        public int lang_id {get;set;}
        public int page_id {get;set;}
        public int record_id {get;set;}
        public DateTime date_ { get; set; }
        public DateTime time_ { get; set; }
        public string note { get; set; }   
        public byte [] object_ {get;set;}
        public int log_file_id {get;set;}
        }

        public class Result
        {
            public bool Successful { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
        }        

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            IShamelDatabase _con;
            public CommandHandler(IShamelDatabase con)=>_con=con;
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                using (var trans = _con.BeginTransaction(db))
                {
                    try
                    {
                        var obj = request.Adapt<LogTrialM>();
                        bool saveTrialRes = await Common.SaveLog(db, trans, request.operation_id, request.company_id, request,
                                           request.page_id,
                                           request.user_id, 0, request.note);   
                        if(saveTrialRes) trans.Commit();                                        
                        else trans.Rollback();
                        return new Result { Successful = saveTrialRes};
                    }
                    catch (Exception err)
                    {
                        trans.Rollback();
                        throw err;
                    }
                }
            }
        }
    }
}