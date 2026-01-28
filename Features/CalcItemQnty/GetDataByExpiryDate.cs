using SHLAPI.Database;
using MediatR;
using static CalcItemQnty_M;

namespace SHLAPI.Features.CalcItemQnty
{
    public class GetDataByExpiryDateF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int storeId { get; set; }
            public int itemId { get; set; }
            public int unitId { get; set; }
            public DateTime qtyExpiryDate { get; set; }
        }
        public class Result
        {
            public IEnumerable<StockQtyBalanceItem> _obj { get; set; }
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
                    var _obj = await CalcItemQnty_M.GetDataByExpiryDate(db, trans,request.itemId,request.unitId,request.qtyExpiryDate,request.storeId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}