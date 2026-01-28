using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;

namespace SHLAPI.Features.Accounts
{
    public class GetAccountBalanceF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string fromAccount { get; set; }
            public string toAccount { get; set; }
            public float fromValue { get; set; }
            public float toValue { get; set; }
            public int isBaseCurr { get; set; }
            public int withSelectedCurrency { get; set; }
            public int currencyId { get; set; }
        }
        public class Result
        {
            public IEnumerable<dynamic> _obj { get; set; }
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
                    var _obj = await Accounts_M.GetAccountBalance(db, request,trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}