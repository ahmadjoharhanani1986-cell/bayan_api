
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.ItemUnits;
using SHLAPI.Features.ItemPrices;

namespace SHLAPI.Controllers
{

    [Route("api/Items")]
    public class ItemsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public ItemsController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetItemUnits")]
        [HttpGet]
        public async Task<IActionResult> GetItemUnits(GetItemUnitsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetItemPrices")]
        [HttpGet]
        public async Task<IActionResult> GetItemPrices(GetItemPricesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetLastPayPriceForItem")]
        [HttpGet]
        public async Task<IActionResult> GetLastPayPriceForItem(GetLastPayPriceForItemF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
