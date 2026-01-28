
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

    [Route("api/Currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public CurrencyController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetCurrencies")]
        [HttpGet]
        public async Task<IActionResult> GetAccounts(GetCurrenciesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }



        [Route("CheckUserTresuryForAccount")]
        [HttpGet]
        public async Task<IActionResult> CheckUserTresuryForAccount(CheckUserTresuryForAccountF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("CopyCurrenciesExchangePrice")]
        [HttpGet]
        public async Task<IActionResult> CopyCurrenciesExchangePrice(CopyCurrenciesExchangePriceF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetCurrencyExchangePrice")]
        [HttpGet]
        public async Task<IActionResult> GetCurrencyExchangePrice(GetCurrencyExchangePriceF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetUsersTreasuryRightsBoxes")]
        [HttpGet]
        public async Task<IActionResult> GetUsersTreasuryRightsBoxes(GetUsersTreasuryRightsBoxesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
