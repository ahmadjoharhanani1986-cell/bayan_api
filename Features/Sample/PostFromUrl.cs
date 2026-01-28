using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SHLAPI.Features.Sample
{
    public class PostFromUrl
    {
        public class Command : IRequest<Result>
        {
            public int PostInt { get; set; }
            public string PostString { get; set; }
            public DateTime PostDateTime { get; set; }
            public double PostDouble { get; set; }
        }

        public class Result
        {
            public int PostInt { get; set; }
            public string PostString { get; set; }
            public DateTime PostDateTime { get; set; }
            public double PostDouble { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                return new Result()
                {
                    PostInt = request.PostInt,
                    PostString = request.PostString,
                    PostDateTime = request.PostDateTime,
                    PostDouble = request.PostDouble
                };
            }
        }
    }
}