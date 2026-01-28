
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.InvoiceVoucher;
using static SHLAPI.Models.InvoiceVoucher.InvoiceVoucherGetData_M;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class SaveInvoiceVoucherF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public Voucher voucherObj { get; set; }
            public List<Journal> journalList { get; set; }
            public List<VouchersItemsAndServices> vouchersItemsAndServicesList { get; set; }
        }
        public class Result
        {
            public VoucherResult _obj { get; set; }
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
                    var _obj = await InvoiceVoucherGetData_M.SaveInvoiceVoucher(db, trans,request);
                    if (_obj !=null && _obj.result) trans.Commit();
                    else trans.Rollback();
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}