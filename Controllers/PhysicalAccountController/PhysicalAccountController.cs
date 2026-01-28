
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.PhysicalAccount;

namespace SHLAPI.Controllers
{

    [Route("api/PhysicalAccount")]
    public class PhysicalAccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public PhysicalAccountController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetPhysicalAccount")]
        [HttpGet]
        public async Task<IActionResult> GetPhysicalAccount(GetPhysicalAccountF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
