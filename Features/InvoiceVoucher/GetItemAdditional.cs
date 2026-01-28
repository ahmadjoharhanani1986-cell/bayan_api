
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.InvoiceVoucher;
using SHLAPI.Models.GetItemByIdOrNo;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class GetItemAdditionalF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int itemId { get; set; }
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
                    var _obj = await GetItemByIdOrNo_M.GetItemAdditional(db,request.itemId,trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}