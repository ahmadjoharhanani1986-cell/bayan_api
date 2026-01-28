using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.Accounts;
using SHLAPI.Models.Currency;
using SHLAPI.Models.Settings;

namespace SHLAPI.Features.SLSettings
{
    public class GetSLSettingsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
        }
        public class Result
        {
            public IEnumerable<Settings_M> _obj { get; set; }
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
                    var _obj = await Settings_M.GetData(db, trans);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}