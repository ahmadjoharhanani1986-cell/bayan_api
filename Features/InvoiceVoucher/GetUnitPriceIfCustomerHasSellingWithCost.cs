
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.SearchItems;
using SHLAPI.Models.InvoiceVoucher;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class GetUnitPriceIfCustomerHasSellingWithCostF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int  itemId { get; set; }
        }
        public class Result
        {
            public dynamic _obj { get; set; }
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
                    var _obj = await InvoiceVoucher_M.GetUnitPriceIfCustomerHasSellingWithCost(db, trans, request.itemId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}