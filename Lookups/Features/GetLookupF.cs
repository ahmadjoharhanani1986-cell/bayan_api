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
    public class GetLookupF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }

            public string tableName { get; set; }
        }

        public class QueryHandler : FeatureHandlerBase, IRequestHandler<Query, Result>
        {
            public QueryHandler(IShamelDatabase con) : base(con)
            {
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                using (var trans = _con.BeginTransaction(db))
                {
                    try
                    {
                        var result = new Result(true);

                        //get project from DB
                        LookupM lookup = await LookupM.Get(db, trans, request.tableName, request.id, request.navigationType);

                        result.dataObject = lookup;

                        //add log
                        // bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Query, request, (int)Pages.Lookup, request.user_id, request.id, request.tableName);
                        // if (!saveTrialRes) result.isSucceeded = false;

                        if (result.isSucceeded)
                            trans.Commit();
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


    public class LookupDTO : LookupM
    {
        public LookupDTO(string tableName):base(tableName)
        {
        }
    }
}