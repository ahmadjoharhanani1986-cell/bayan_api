
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using System.Data;
namespace SHLAPI.Features.VoucherQabdSarfGetData
{
    public class VoucherQabdSarfGetDataF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int userId { get; set; }
            public string type { get; set; }
            public bool _getMaxNoFromService { get; set; }
            public int voucherId { get; set; }
            public DateTime _date { get; set; }
            public string voucherNo { get; set; }
              public string _viewName { get; set; }
        }
        public class Result
        {
            public VoucherQabdSarfGetData_M _obj { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.GetData(db, trans, request._getMaxNoFromService, request.voucherId, request.userId,
                                                                      request.type, request._date,request.voucherNo,request._viewName);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}