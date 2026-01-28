using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;

namespace SHLAPI.Features.Accounts
{
    public class GetAccountsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string accountPrefix { get; set; }
            public int currency_id { get; set; }
            public string from_code { get; set; }
            public string to_code { get; set; }
        }
        public class Result
        {
            public IEnumerable<Accounts_M> _obj { get; set; }
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
                    var _obj = await Accounts_M.GetData(db, trans, request);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}