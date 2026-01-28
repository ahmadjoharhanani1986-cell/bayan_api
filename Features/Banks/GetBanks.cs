using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Banks;

namespace SHLAPI.Features.Banks
{
    public class GetBanksF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string code { get; set; }
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
                    var _obj = await Banks_M.GetData(db, trans,request.code);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}