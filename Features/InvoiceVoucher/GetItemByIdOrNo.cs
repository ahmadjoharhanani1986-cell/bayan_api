
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.SearchItems;
using SHLAPI.Models.GetItemByIdOrNo;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class GetItemByIdOrNoF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string type { get; set; }
            public bool getSellUnit { get; set; }
            public int accountId { get; set; }
            public int itemUnit { get; set; }
            public int itemId { get; set; }
            public int storeId { get; set; }
            public string itemNo { get; set; }
            public string itemBarCode { get; set; }
            public int findItemBy { get; set; } // 1 id , 2 no , 3 barcode
            public DateTime voucherDate { get; set; }
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
                    var _obj = await GetItemByIdOrNo_M.GetData(db, trans, request);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}