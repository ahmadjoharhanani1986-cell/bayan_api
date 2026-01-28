
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class CheckIfHaveManualVoucherNoF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string manualVoucherNo { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.CheckIfHaveManualVoucherNo(db,trans,request.manualVoucherNo,request.type);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}