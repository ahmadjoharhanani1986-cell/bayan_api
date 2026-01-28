using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;

namespace SHLAPI.Features.Accounts
{
    public class SearchAccountsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string additionalConditions { get; set; }
            public string accountName { get; set; }
            public string tableName { get; set; }
            public bool newViewProp { get; set; }
            public bool isFillteredByStopped { get; set; }
            public bool isAccount { get; set; }
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
                    var _obj = await SearchAccount_M.SearchAccounts(db, trans,request);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}