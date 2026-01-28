using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;
using SHLAPI.Models.InvoiceVoucher;

namespace SHLAPI.Features.Accounts
{
    public class GetAccountAllDataF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int coaId { get; set; }
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
                    var _obj = await InvoiceVoucherGetData_M.GetAccountAllData(db, request.coaId, trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}