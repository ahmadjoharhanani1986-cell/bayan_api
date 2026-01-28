
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.InvoiceVoucher;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class CheckConnectedInvoiceF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int voucherId { get; set; }
        }
        public class Result
        {
            public bool _obj { get; set; }
            public string _no { get; set; }
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
                    var returnObj = await InvoiceVoucherGetData_M.CheckConnectedInvoice(db,trans,request.voucherId);
                    return new Result { _obj = returnObj._obj,_no=returnObj._no };
                }
            }
        }
    }
}