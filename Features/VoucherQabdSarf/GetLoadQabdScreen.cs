
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using System.Data;
namespace SHLAPI.Features.GetLoadQabdScreen
{
    public class GetLoadQabdScreenF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
          public int userId { get; set; }
          public  int currencyId { get; set; }
          public  string type { get; set; }
        }
        public class Result
        {
            public VoucherQabdSarf_M _obj { get; set; }
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
                    var _obj = await VoucherQabdSarf_M.LoadQabdSarfScreen(db, trans, request.user_id,request.currencyId, request.type);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}