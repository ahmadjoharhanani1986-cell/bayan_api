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
    public class UsersQueryIdNameF
    {
        public class Query : FeatureBase, IRequest<QueryResult>
        {
            public int status { get; set; }
        }

        public class QueryResult : Result
        {
            public IEnumerable<UserM> usersList { get; set; }
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
                    result.dataObject = await UserM.GetListIdName(db, null, request.status);


                    return result;
                }
            }
        }
    }
}