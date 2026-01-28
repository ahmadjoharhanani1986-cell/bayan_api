
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.VoucherQabdSarf;
using System.Data;
using SHLAPI.Models.InvoiceVoucher;
using SHLAPI.Models.UserInfo;
using Dapper;
namespace SHLAPI.Features.LoadInvoiceData
{
    public class LoadInvoiceDataF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
          public int userId { get; set; }
          public  int currencyId { get; set; }
          public  string type { get; set; }
        }
        public class Result
        {
            public InvoiceVoucher_M _obj { get; set; }
            public dynamic _userInfoObj { get; set; }
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
                    var _obj = await InvoiceVoucher_M.LoadInvoiceData(db, trans, request.user_id,request.currencyId, request.type);
                    var userInfoList = await UserInfo_M.GetData(db, trans, request.user_id);
                    return new Result { _obj = _obj,_userInfoObj= userInfoList};
                }
            }
        }
    }
}