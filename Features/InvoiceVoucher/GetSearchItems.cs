
using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.SearchItems;
namespace SHLAPI.Features.InvoiceVoucher
{
    public class GetSearchItemsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string type { get; set; }
            public string filterText { get; set; }
            public bool _getSusspended { get; set; }
            public bool chkIncludeBarCodeWithSearch { get; set; }
            public bool showNotes { get; set; }
            public bool showPrice { get; set; }
            public bool showCurrency { get; set; }
              public bool getItemImg { get; set; }
                public bool dontGetService { get; set; }
        }
        public class Result
        {
            public IEnumerable<SearchItems_M> _obj { get; set; }
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
                    var _obj = await SearchItems_M.GetData(db, trans, request);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}