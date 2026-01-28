
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.GetLoadQabdScreen;
using SHLAPI.Features.InvoiceVoucher;
using SHLAPI.Features.LoadInvoiceData;
using SHLAPI.Features.InvoiceGetData;

namespace SHLAPI.Controllers
{

    [Route("api/InvoiceVoucher")]
    public class InvoiceVoucherController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public InvoiceVoucherController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }
        [Route("LoadInvoiceData")]
        [HttpGet]
        public async Task<IActionResult> LoadInvoiceData(LoadInvoiceDataF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("InvoiceGetData")]
        [HttpPost]
        public async Task<IActionResult> InvoiceGetData([FromBody] InvoiceGetDataF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        // [Route("DeleteInvoiceVoucher")]
        // [HttpGet]
        // public async Task<IActionResult> DeleteInvoiceVoucher(DeleteInvoiceVoucherF.Query qry)
        // {
        //     Common.FillDefault(qry, _context);
        //     var result = await _mediator.Send(qry);
        //     return Ok(result);
        // }
        [Route("SaveInvoiceVoucher")]
        [HttpPost]
        public async Task<IActionResult> SaveInvoiceVoucher([FromBody] SaveInvoiceVoucherF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("SearchItems")]
        [HttpGet]
        public async Task<IActionResult> SearchItems(GetSearchItemsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetItemByIdOrNo")]
        [HttpGet]
        public async Task<IActionResult> GetItemByIdOrNo(GetItemByIdOrNoF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetUnitPriceIfCustomerHasSellingWithCost")]
        [HttpGet]
        public async Task<IActionResult> GetUnitPriceIfCustomerHasSellingWithCost(GetUnitPriceIfCustomerHasSellingWithCostF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("CheckManualNo")]
        [HttpGet]
        public async Task<IActionResult> CheckManualNo(CheckManualNoF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("CheckConnectedInvoice")]
        [HttpGet]
        public async Task<IActionResult> CheckConnectedInvoice(CheckConnectedInvoiceF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetItemAdditional")]
        [HttpGet]
        public async Task<IActionResult> GetItemAdditional(GetItemAdditionalF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetCostCenters")]
        [HttpGet]
        public async Task<IActionResult> GetCostCenters(GetCostCentersF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetStores")]
        [HttpGet]
        public async Task<IActionResult> GetStores(GetStoresF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
