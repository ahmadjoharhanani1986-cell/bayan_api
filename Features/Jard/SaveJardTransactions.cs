using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.Jard;
using static SHLAPI.Models.Jard.JardTransaction_M;

namespace SHLAPI.Features.Jard
{
    public class SaveJardTransactionsF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int jardId { get; set; }
            public int storeId { get; set; }
            public List<JardTransObj> list { get; set; }
        }
        public class Result
        {
            public bool _obj { get; set; }
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
                    var resultSave = await JardTransaction_M.SaveOrUpdateAllJardItems(db, trans, request.jardId,request.storeId,request.user_id,request.list);
                    if (resultSave)
                        trans.Commit();
                    else trans.Rollback();
                    return new Result { _obj = resultSave };
                }
            }
        }
    }
}