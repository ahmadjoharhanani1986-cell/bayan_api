
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class CheckVoucherHaveBillPayF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int voucherId { get; set; }
            public string type { get; set; }
        }
        public class Result
        {
            public bool _obj { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.CheckVoucherHaveBillPay(db,trans,request.voucherId);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}