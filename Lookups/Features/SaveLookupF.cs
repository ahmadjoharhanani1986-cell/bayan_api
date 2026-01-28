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
    public class SaveLookupF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }
            public int crm_id { get; set; }

            public string name { get; set; }

            public int status { get; set; }

            public string tableName { get; set; }
        }

        public class SaveProductResult : Result { }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                //RuleFor(c => c.name).NotEmpty().WithMessage(Messages.name_is_mandatory);
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
                        LookupM obj = new LookupM(request.tableName) { id = request.id, name = request.name, status = request.status, crm_id = request.crm_id };
                        var result = new Result(true);

                        if (request.tableName == "employees_tbl")
                            result += await LookupM.SaveEmployee(db, trans, obj);
                        else
                            result += await LookupM.Save(db, trans, request.tableName, obj);

                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, Common.GetOperation(obj.id), request, (int)Pages.Lookup, request.user_id, obj.id, request.tableName);
                        if (!saveTrialRes) result.isSucceeded = false;
                        if (result.isSucceeded)
                        {
                            trans.Commit();
                            result.dataObject = obj;
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