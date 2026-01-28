using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SHLAPI.Database;
using SHLAPI.Models.LogFile;
using MediatR;

namespace SHLAPI.Features.LogFile
{
    public class GetRecordTransactionsLogF
    {
        public class Query : IRequest<Result>
        {
            public int company_id {get;set;}
            public int record_id { get; set; }
            public int language_id { get; set; }
            public int page_id { get; set; }
            public int user_id { get; set; }
        }
        public class Result
        {
            public IEnumerable<RecordTransactionsLogM> _recordTransactionsLog { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            IShamelDatabase _con;
            public QueryHandler(IShamelDatabase con)=>_con=con;
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                using (var trans = _con.BeginTransaction(db))
                {
                    var recordTransactionsLog = await RecordTransactionsLogM.GetRecordTransactionsLog(db, request);
                    return new Result { _recordTransactionsLog = recordTransactionsLog };
                }
            }
        }
    }
}