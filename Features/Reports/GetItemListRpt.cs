using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.ItemListRpt;

namespace SHLAPI.Features.Reports
{
    public class GetItemListRptF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public string fromCode { get; set; }
            public string toCode { get; set; }
            public string itemName { get; set; }
            public string supplier { get; set; }
            public bool chkExpiry { get; set; }
            public bool chkLess { get; set; }
            public bool chkEqual { get; set; }
            public bool chkSuspendedItem { get; set; }
            public bool chkItemDetails { get; set; }
            public bool chkItemTrans { get; set; }
            public int storeId { get; set; }
            public bool chkAbove { get; set; }
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
                    var _obj = await ItemListRpt_M.GetData(db, trans, request);
                    return new Result { _obj = _obj };
                }
            }
        }
    }
}