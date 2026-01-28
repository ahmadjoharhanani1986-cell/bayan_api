
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using System.Data;
using static SHLAPI.Models.VoucherQabdSarf.VoucherQabdSarfGetData_M;
namespace SHLAPI.Features.VoucherQabdSarf
{
    public class DeleteQabdSarfVoucherF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int userId { get; set; }
            public string type { get; set; }
            public int voucherId { get; set; }
            public DateTime _date { get; set; }
            public string voucherNo { get; set; }
            public string _viewName { get; set; }
               public string deletedNote { get; set; }
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
                    var _obj = await VoucherQabdSarfGetData_M.DeleteQabdSarfVoucher(db, trans, request.voucherId, request.userId,
                                                                      request.type, request._date,request.voucherNo,request._viewName,request.deletedNote);
                    if (_obj.result) trans.Commit();
                    else trans.Rollback();
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}