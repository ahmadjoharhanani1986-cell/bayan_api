using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SHLAPI.Features.Sample
{
    [Route("api/samples")]
    public class SampleController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public SampleController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpGet("getmixed/{FromUrl}")]
        public async Task<IActionResult> GetFromMixed(GetFromMixed.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("getFromParameter")]
        public async Task<IActionResult> GetFromParameter(GetFromParameter.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("getFromUrl/{GetInt}/{getString}/{getDatetime}/{getDouble}")]
        public async Task<IActionResult> GetFromUrl(GetFromUrl.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("postFromBody")]
        public async Task<IActionResult> PostFromBody([FromBody] PostFromBody.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("postFromUrl/{PostInt}/{postString}/{postDatetime}/{postDouble}")]
        public async Task<IActionResult> PostFromUrl([FromRoute] PostFromUrl.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("postMixed/{postInt}")]
        public async Task<IActionResult> PostMixed([FromRoute] int postInt, [FromBody] PostMixed.Command command)
        {
            command.PostInt = postInt;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}