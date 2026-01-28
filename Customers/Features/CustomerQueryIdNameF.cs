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
    public class CustomerQueryIdNameF
    {
        public class Query : FeatureBase, IRequest<QueryResult>
        {
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
                    
                    //await Common.CheckPermission(db,null,request.user_id,39);

                    //get from DB
                    result.dataObject = await CustomersM.GetListIdName(db, null);

                    return result;
                }
            }
        }
    }
}