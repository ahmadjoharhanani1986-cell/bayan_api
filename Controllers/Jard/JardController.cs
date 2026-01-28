
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.Jard;

namespace SHLAPI.Controllers
{

    [Route("api/Jard")]
    public class JardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public JardController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetJard")]
        [HttpGet]
        public async Task<IActionResult> GetJard(GetJardF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
       [Route("GetJardTransactions")]
        [HttpGet]
        public async Task<IActionResult> GetJardTransactions(GetJardTransactionsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("SaveJardTransactions")]
        [HttpPost]
        public async Task<IActionResult> SaveJardTransactions([FromBody] SaveJardTransactionsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        
    }
}
