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
    public class UpdateLanguageF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public int language_id { get; set; }
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

                        result += await UserM.UpdateLanguage(db,trans,request.user_id,request.language_id);
                        
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Update , request, (int)Pages.User, request.user_id, request.user_id, "change langualge to "+request.language_id);
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
        }
    }
}