
using SHLAPI.Database;
using SHLAPI.Models.Authentication;
using MediatR;

namespace SHLAPI.Features
{
    public class Authentication_F
    {
        public class Query : FeatureBase,IRequest<AuthenticationResult>
        {
            public int id { get; set; }
            public string login_name  { get; set; }
            public string pwd { get; set; }
        }

        public class QueryHandler : FeatureHandlerBase,IRequestHandler<Query, AuthenticationResult>
        {
            IMasterDatabase conMaster;
            public QueryHandler(IShamelDatabase _con,IMasterDatabase conMaster) : base(_con)
            {
                _conMaster = conMaster;
            }

            public async Task<AuthenticationResult> Handle(Query request, CancellationToken cancellationToken)
            {
                using (var db = _conMaster.Open())
                {
                    using (var dbShamelRealData = _con.Open())
                    {
                        var result = await Authentication_M.CheckAuthentication(db, dbShamelRealData, request.login_name, request.pwd);

                        return result;
                    }
                }
            }
        }
    }
    public class AuthenticationResult : Result
    {
        public AuthenticationResult() : base() { }
        public AuthenticationResult(bool isSucceeded) : base(isSucceeded) { }
        public int user_id { get; set; }
        public string user_name { get; set; }
        public int main_role { get; set; }
        public string main_role_name { get; set; }
        public string token { get; set; }
        public int language_id { get; set; }
        public bool userActiveInWeb { get; set; }
    }
}