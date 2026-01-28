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
    public class GetTasksTypesF
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

                    //get from DB
                    var tmpList = await LookupM.GetList(db, null, "tasks_types_tbl",null,null);
                    var lst = tmpList.ToList();
                    lst.Add(new LookupM("tasks_types_tbl") { id = 99, name = "Analysis chnage" });
                    result.dataObject = lst;

                    return result;
                }
            }
        }
    }
}