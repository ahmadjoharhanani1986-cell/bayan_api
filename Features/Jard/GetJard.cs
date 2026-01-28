using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Jard;

namespace SHLAPI.Features.Jard
{
    public class GetJardF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
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
                    var _obj = await Jard_M.GetData(db, trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}