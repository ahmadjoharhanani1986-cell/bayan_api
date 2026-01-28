
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Currency;

namespace SHLAPI.Features.Accounts
{
    public class CopyCurrenciesExchangePriceF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public DateTime _date { get; set; }
            public int currencyId { get; set; }
        }
        public class Result
        {
            public bool _obj { get; set; }
            public double _exchangPrice { get; set; }
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
                    var _obj = await Currency_M.CopyCurrenciesExchangePrice(db, trans, request._date);
                    var _exchangPrice = 1.0;
                    if (_obj)
                    {
                      _exchangPrice = await Currency_M.GetCurrencyExchangePrice(db, trans, request.currencyId, request._date);
                    }
                    trans.Commit();
                    return new Result { _obj = _obj, _exchangPrice = _exchangPrice };
                }
            }
        }
    }
}