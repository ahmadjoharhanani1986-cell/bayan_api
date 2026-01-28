
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using System.IO;
using Newtonsoft.Json;
using SHLAPI.Features.StatmentOfAccountsRpt;
using SHLAPI.Features.Accounts;
using SHLAPI.Features.SLSettings;

namespace SHLAPI.Controllers
{

    [Route("api/Settings")]
    public class SLSettingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public SLSettingsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetSLSettings")]
        [HttpGet]
        public async Task<IActionResult> GetSLSettings(GetSLSettingsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
     
    }
}
