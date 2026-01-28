using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;
using static SHLAPI.Models.Accounts.Accounts_M;

namespace SHLAPI.Features.Accounts
{
    public class CalcAccountRasidF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int accountId { get; set; }
            public int accountCurrency { get; set; }
        }
        public class Result
        {
            public AccountBalanceResult _obj { get; set; }
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
                    var _obj = await Accounts_M.CalcAccountRasid(db,trans,request.accountId,request.accountCurrency);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}