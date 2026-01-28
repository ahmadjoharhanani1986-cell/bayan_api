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
    public class DeleteUserF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }
        }

        public class DeleteResult : Result { }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
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

                        await Common.CheckPermission(db,trans,request.user_id,78);

                        result += await UserM.Delete(db, trans, request.id);
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Delete, request, (int)Pages.User, request.user_id, obj.id, "");
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
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}