
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.Banks;

namespace SHLAPI.Controllers
{

    [Route("api/Banks")]
    public class BanksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public BanksController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetBanks")]
        [HttpGet]
        public async Task<IActionResult> GetBanks(GetBanksF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
