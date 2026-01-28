using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using Mapster;
using MediatR;
using SHLAPI.Database;
using SHLAPI.Models;

namespace SHLAPI.Features
{
    public class ChangePasswordF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public string old_password { get; set; }
            public string new_password { get; set; }
        }


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
                        var result = new Result(true);

                        result += await Validate(db,trans,request.user_id,request.new_password,request.old_password);

                        if(result.isSucceeded)
                        {
                            result += await UserM.ChangePassword(db,trans,request.user_id,request.new_password);
                        }
                        
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Update , request, (int)Pages.User, request.user_id, request.user_id, "password changed");
                        if (!saveTrialRes) result.isSucceeded = false;
                        if (result.isSucceeded)
                        {
                            trans.Commit();
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

            private async Task<Result> Validate(IDbConnection db, IDbTransaction trans, int userId, string newPassword, string oldPassword)
            {
                Result result = new Result();

                result += await UserM.CheckPassword(db,trans,userId,oldPassword);

                return result;
            }            
        }
    }
}