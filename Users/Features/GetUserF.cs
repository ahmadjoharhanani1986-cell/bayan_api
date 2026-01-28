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
    public class GetUserF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int id { get; set; }
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

                        await Common.CheckPermission(db,trans,request.user_id,35);

                        //get from DB
                        result.dataObject = await UserM.Get(db, trans, request.id, request.navigationType);

                        //add log
                        bool saveTrialRes = await Common.SaveTrialLog(db, trans, (int)OperationTypes.Query, request, (int)Pages.User, request.user_id, request.id, "");
                        if (!saveTrialRes) result.isSucceeded = false;

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
}