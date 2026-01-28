using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Database;
using SHLAPI.Models;

namespace SHLAPI.Features
{
    public class DeleteLookupF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }

            public string tableName { get; set; }

        }

        public class DeleteProjectAttachmentResult : Result { }

        public class CommandValidator : AbstractValidator<Command>
        {
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

                        result += await LookupM.Delete(db, trans, request.tableName, request.id);

                        string note = "delete "+request.tableName+" for id=" + request.id;
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Delete, request, (int)Pages.Lookup, request.user_id, request.id, note);
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