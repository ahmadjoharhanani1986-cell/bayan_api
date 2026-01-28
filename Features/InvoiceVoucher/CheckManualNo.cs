
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.InvoiceVoucher;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class CheckManualNoF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string manualNo { get; set; }
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
                    var _obj = await InvoiceVoucher_M.CheckManualNo(db,trans,request.manualNo,request.type);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}