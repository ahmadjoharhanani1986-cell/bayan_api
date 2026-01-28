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
    public class LookupsQueryF
    {
        public class Query : FeatureBase, IRequest<QueryResult>
        {
            public int? id { get; set; }

            public int? status { get; set; }

            public string tableName { get; set; }
        }

        public class QueryResult : Result
        {
        }

        public class QueryHandler : FeatureHandlerBase, IRequestHandler<Query, QueryResult>
        {
            public QueryHandler(IShamelDatabase con) : base(con)
            {
            }

            public async Task<QueryResult> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                {
                    var result = new QueryResult();

                    //get from DB
                    result.dataObject = await LookupM.GetList(db, null, request.tableName,request.status,request.id);

                    //add log
                    // bool saveTrialRes = await Common.SaveTrialLog(db, null, (int)OperationTypes.Query, request, (int)Pages.LookupsQuery, request.user_id, 0, request.tableName);
                    // if (!saveTrialRes) result.isSucceeded = false;

                    return result;
                }
            }
        }
    }
}