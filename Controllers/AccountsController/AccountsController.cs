
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using System.IO;
using Newtonsoft.Json;
using SHLAPI.Features.StatmentOfAccountsRpt;
using SHLAPI.Features.Accounts;

namespace SHLAPI.Controllers
{

    [Route("api/Accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public AccountsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetAccounts")]
        [HttpGet]
        public async Task<IActionResult> GetAccounts(GetAccountsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetAccountBalance")]
        [HttpGet]
        public async Task<IActionResult> GetAccountBalance(GetAccountBalanceF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("SearchAccounts")]
        [HttpGet]
        public async Task<IActionResult> SearchAccounts(SearchAccountsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetAccountAllData")]
        [HttpGet]
        public async Task<IActionResult> GetAccountAllData(GetAccountAllDataF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
               [Route("CalcAccountRasid")]
        [HttpGet]
        public async Task<IActionResult> CalcAccountRasid(CalcAccountRasidF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
