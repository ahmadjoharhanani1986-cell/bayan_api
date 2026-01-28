
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using SHLAPI.Models.Settings;
using SHLAPI.Models.SearchVouchers;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class GetSearchVouchersF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string type { get; set; }
            public string filterText { get; set; }
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
                     var _obj = await SearchVouchers_M.GetData(db,trans,request.type,request.filterText);
                     return new Result { _obj = _obj };
                }
            }
        }
    }
}