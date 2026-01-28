using SHLAPI.Database;
using MediatR;

namespace SHLAPI.Features.CalcItemQnty
{
    public class GetCalcItemQntyF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int storeId { get; set; }
            public int itemId { get; set; }
            public int unitId { get; set; }
        }
        public class Result
        {
            public CalcItemQnty_M _obj { get; set; }
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
                    var _obj = await CalcItemQnty_M.GetData(db, trans,request.itemId,request.unitId,request.storeId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}