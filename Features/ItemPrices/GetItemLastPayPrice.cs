using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.ItemPrices;

namespace SHLAPI.Features.ItemPrices
{
    public class GetLastPayPriceForItemF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int itemId { get; set; }
            public int langId { get; set; }
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
                    var _obj = await ItemPrices_M.GetLastPayPriceForItem(db, trans,request.itemId,request.langId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}