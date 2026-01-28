
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class CheckDateOfVoucherF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string type { get; set; }
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
                    List<Settings_M> settingList = (List<Settings_M>)await Settings_M.GetData(db, trans);
                    var _obj = await VoucherQabdSarfGetData_M.CheckDateOfVoucher(db,request.type,trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}