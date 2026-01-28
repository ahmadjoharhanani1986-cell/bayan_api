using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SHLAPI.Features.Sample
{
    public class GetFromParameter
    {
        public class Query : IRequest<Result>
        {
            public int GetInt { get; set; }
            public string GetString { get; set; }
            public DateTime GetDateTime { get; set; }
            public double GetDouble { get; set; }

            //CAN'T GET OBJECT FROM PARAMETER OR URL
            public ObjectParameter GetObject { get; set; }
        }

        public class ObjectParameter
        {
            public int Id { get; set; }
        }

        public class Result
        {
            public int GetInt { get; set; }
            public string GetString { get; set; }
            public DateTime GetDateTime { get; set; }
            public double GetDouble { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                return new Result
                {
                    GetInt = request.GetInt,
                    GetString = request.GetString,
                    GetDateTime = request.GetDateTime,
                    GetDouble = request.GetDouble
                };
            }
        }
    }
}