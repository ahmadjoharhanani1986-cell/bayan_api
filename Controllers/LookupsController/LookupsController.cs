
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.Lookups;

namespace SHLAPI.Controllers
{

    [Route("api/Lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public LookupsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetUnits")]
        [HttpGet]
        public async Task<IActionResult> GetUnits(GetUnitsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
