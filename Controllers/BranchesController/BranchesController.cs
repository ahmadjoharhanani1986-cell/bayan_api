
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.Branches;

namespace SHLAPI.Controllers
{

    [Route("api/Branches")]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public BranchesController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetBranches")]
        [HttpGet]
        public async Task<IActionResult> GetBranches(GetBranchesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
