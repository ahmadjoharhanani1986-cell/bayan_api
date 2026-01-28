using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Branches;

namespace SHLAPI.Features.Branches
{
    public class GetBranchesF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string code { get; set; }
             public int  bankId { get; set; }
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
                    var _obj = await Branches_M.GetData(db, trans,request.code,request.bankId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}