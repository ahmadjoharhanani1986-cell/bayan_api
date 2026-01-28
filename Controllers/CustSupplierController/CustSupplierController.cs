
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.CustSupplier;
using SHLAPI.Features.InvoiceVoucher;

namespace SHLAPI.Controllers
{

    [Route("api/CustSupplier")]
    public class CustSupplierController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public CustSupplierController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetCustSupplier")]
        [HttpGet]
        public async Task<IActionResult> GetCustSupplier(GetCustSupplierF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        
    }
}
