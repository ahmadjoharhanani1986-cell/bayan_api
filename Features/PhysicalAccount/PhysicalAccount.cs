using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.PhysicalAccount;

namespace SHLAPI.Features.PhysicalAccount
{
    public class GetPhysicalAccountF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int curr_id { get; set; }
              public int id { get; set; }
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
                    var _obj = await PhysicalAccount_M.GetData(db, trans,request.curr_id,request.id);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}