
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using System.Data;
using static SHLAPI.Models.VoucherQabdSarf.VoucherQabdSarfGetData_M;
namespace SHLAPI.Features.VoucherQabdSarf
{
    public class SaveQabdSarfVoucherF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public Voucher voucherObj { get; set; }
            public List<Journal> journalList { get; set; }
            public List<Check> checkList { get; set; }
            public List<CheckTrans> checkTransList { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.SaveVouchersQabdAndSarf(db, trans,request);
                    if (_obj !=null && _obj.result) trans.Commit();
                    else trans.Rollback();
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}