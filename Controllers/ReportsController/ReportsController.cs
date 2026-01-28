
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.Reports;

namespace SHLAPI.Controllers
{

    [Route("api/Reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public ReportsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetItemListRpt")]
        [HttpGet]
        public async Task<IActionResult> GetItemListRpt(GetItemListRptF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
