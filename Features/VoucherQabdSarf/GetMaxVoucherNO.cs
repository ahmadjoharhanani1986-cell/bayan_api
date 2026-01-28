
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class GetMaxVoucherNOF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int userId { get; set; }
            public int currencyId { get; set; }
            public string type { get; set; }
            public  DateTime voucherDate { get; set; }
        }
        public class Result
        {
            public string _obj { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.GetMaxVoucherNOAsync(request.type,db,request.user_id,request.voucherDate,settingList,trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}