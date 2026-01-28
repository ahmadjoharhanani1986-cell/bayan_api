using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Mapster;
using MediatR;
using SHLAPI.Database;
using SHLAPI.Models;

namespace SHLAPI.Features
{
    public class SaveUserF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }

            public string name { get; set; }

            public string login_name { get; set; }

            public int main_role { get; set; }

            public string pwd { get; set; }

            public string email { get; set; }

            public string mobile { get; set; }

            public int is_system_admin { get; set; }

            public int status { get; set; }
        }

        public class SaveProductResult : Result { }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                //RuleFor(c => c.account_id).NotEqual(0).WithMessage("");
                //RuleFor(c => c.name).NotNull().WithMessage("");
            }
        }

        public class CommandHandler : FeatureHandlerBase, IRequestHandler<Command, Result>
        {
            public CommandHandler(IShamelDatabase con) : base(con)
            {
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                using (var trans = _con.BeginTransaction(db))
                {
                    try
                    {
                        var obj = request.Adapt<UserM>();
                        var result = new Result(true);

                        await Common.CheckPermission(db,trans,request.user_id,35);

                        UserM usr=null;
                        
                        if(obj.id>0 && obj.pwd.Trim().Length==0)
                        {
                            usr=await UserM.GetPassword(db,trans,obj.id);
                            obj.pwd=usr.pwd;
                        }
                        if(await UserM.CheckIfDisabled(db,trans,obj.id))
                        {
                            result.isSucceeded=false;
                            result.mainError=new ErrorDescription(){description="user is disabled"};
                            return result;
                        }

                        result += await UserM.Save(db, trans, obj);
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, Common.GetOperation(obj.id), request, (int)Pages.Product, request.user_id, obj.id, "");
                        if (!saveTrialRes) result.isSucceeded = false;
                        if (result.isSucceeded)
                        {
                            trans.Commit();
                            result.dataObject=obj;
                        }
                        else
                            trans.Rollback();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }
}