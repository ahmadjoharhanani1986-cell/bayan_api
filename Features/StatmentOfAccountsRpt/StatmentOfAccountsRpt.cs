using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;
using SHLAPI.Models.StatmentOfAccountsRpt;

namespace SHLAPI.Features.StatmentOfAccountsRpt
{
    public class StatmentOfAccountsRptF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int company_id { get; set; }
            public int record_id { get; set; }
            public int language_id { get; set; }
            public int page_id { get; set; }
            public int base_curr { get; set; }
            public int account_id { get; set; }
            public DateTime from_date { get; set; }
            public DateTime to_date { get; set; }
            public string prevBalanceCaption { get; set; }
            public int withInvoicesDetails { get; set; }
            public int fromRight { get; set; }
            public int withChecksDetails { get; set; }
            public int currency_id { get; set; }
            public int transByCurr { get; set; }
            public int hideEqfal { get; set; }
            public int showManualNo { get; set; }
            public string checksDateFormat { get; set; }
            public int prevDebitCredit { get; set; }
            public int printChecksDetails { get; set; }
            public string accountPrefix { get; set; }
            public string from_code { get; set; }
            public string to_code { get; set; }
            public bool getCheqBalance { get; set; }
        }
        public class Result
        {
            public IEnumerable<StatmentOfAccountsRpt_M> _obj { get; set; }
               public IEnumerable<StatmentOfAccountsRpt_M> _objChecqBalance { get; set; }
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
                    var obj = await StatmentOfAccountsRpt_M.GetData(db, trans,request);
                    List<StatmentOfAccountsRpt_M> objChecqBalance = null;
                    if (request.getCheqBalance)
                         objChecqBalance = (List<StatmentOfAccountsRpt_M>)await StatmentOfAccountsRpt_M.GetCheqBalance(db, trans, request);
                    return new Result { _obj = obj,_objChecqBalance=objChecqBalance };
                }
            }
        }
    }
}