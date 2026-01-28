
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using Newtonsoft.Json;
using System.IO;

namespace SHLAPI.Controllers
{

    [Route("api/CommonTools")]
    public class CommonToolsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public CommonToolsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("SendGridAsEmail")]
        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> SendGridAsEmail([FromBody]SendGridAsEmailF.Command command)
        {
            Common.FillDefault(command, _context);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
