using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SHLAPI.Features.Sample
{
    public class GetFromMixed
    {
        public class Query : IRequest<Result>
        {
            public int FromUrl { get; set; }
            public int FromParamerter { get; set; }
        }

        public class Result
        {
            public int FromUrl { get; set; }
            public int FromParamerter { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                return new Result()
                {
                    FromUrl = request.FromUrl,
                    FromParamerter = request.FromParamerter
                };
            }
        }
    }
}