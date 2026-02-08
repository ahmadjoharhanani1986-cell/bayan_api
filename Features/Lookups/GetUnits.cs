

using SHLAPI.Models.Lookups;
using SHLAPI.Database;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
namespace SHLAPI.Features.Lookups
{
    public class GetUnitsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int id {get;set;}
            public string state {get;set;}
        }
        public class Result
        {
            public IEnumerable<dynamic> _obj { get; set; }
        }
        public class QueryHandler : IRequestHandler<Query, Result>
        {
            IShamelDatabase _con;
            public QueryHandler(IShamelDatabase con) => _con = con;
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                using (var trans = _con.BeginTransaction(db))
                {
                    var _obj = await Unit_M.GetAll(db, trans,request.id,request.state);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}