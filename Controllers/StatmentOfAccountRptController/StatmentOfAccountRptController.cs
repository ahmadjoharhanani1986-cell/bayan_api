
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using System.IO;
using Newtonsoft.Json;
using SHLAPI.Features.StatmentOfAccountsRpt;

namespace SHLAPI.Controllers
{

    [Route("api/StatmentOfAccountRpt")]
    public class StatmentOfAccountRptController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public StatmentOfAccountRptController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetStatmentOfAccountRpt")]
        [HttpPost]
        public async Task<IActionResult> QueryIdName([FromBody] StatmentOfAccountsRptF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetStatmentOfAccountsRptShap3")]
        [HttpPost]
        public async Task<IActionResult> GetStatmentOfAccountsRptShap3([FromBody] GetStatmentOfAccountsRptShap3F.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
