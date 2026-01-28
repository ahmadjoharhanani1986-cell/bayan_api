
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
using static SHLAPI.Models.VoucherQabdSarf.VoucherQabdSarfGetData_M;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class NavigateVouchersF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int voucherId { get; set; }
            public string type { get; set; }
            public navigateTypeEnum navigateType { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.Navigate(db,trans,request.voucherId,request.navigateType,request.type);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}