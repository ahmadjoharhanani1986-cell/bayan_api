using SHLAPI.Database;
using MediatR;
using SHLAPI.Models.CustSupplier;
using Dapper;

namespace SHLAPI.Features.CustSupplier
{
    public class GetCustSupplierF
    {
        public class Query : FeatureBase, IRequest<Result>
        {
            public int coaId { get; set; }
        }
        public class Result
        {
            public dynamic _obj { get; set; }
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
                    var _obj = await CustSupplier_M.GetData(db, trans, request.coaId);
                    if (_obj != null && _obj.AsList().Count > 0)
                    {
                        return new Result { _obj = _obj.AsList()[0] };
                    }
                    return new Result { _obj = null };
                }
            }
        }
    }
}