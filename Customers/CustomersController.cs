
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using System.IO;
using Newtonsoft.Json;

namespace SHLAPI.Controllers
{

    [Route("api/Customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public CustomersController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("QueryIdName")]
        [HttpPost]
        public async Task<IActionResult> QueryIdName([FromBody] CustomerQueryIdNameF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
