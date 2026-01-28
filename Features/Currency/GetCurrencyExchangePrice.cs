
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Currency;

namespace SHLAPI.Features.Accounts
{
    public class GetCurrencyExchangePriceF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int currencyId { get; set; }
            public DateTime _date { get; set; }
        }
        public class Result
        {
            public double _obj { get; set; }
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
                    var _obj = await Currency_M.GetCurrencyExchangePrice(db, trans,request.currencyId,request._date);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}