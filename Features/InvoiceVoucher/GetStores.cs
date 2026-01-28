
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.InvoiceVoucher;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class GetStoresF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
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
                    var _obj = await InvoiceVoucherGetData_M.GetStores(db,request.user_id,trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}