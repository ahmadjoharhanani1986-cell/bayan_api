
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using System.IO;
using Newtonsoft.Json;
using SHLAPI.Features.GetLoadQabdScreen;
using SHLAPI.Features.VoucherQabdSarfGetData;
using SHLAPI.Models.SearchVouchers;
using SHLAPI.Features.VoucherQabdSarf;

namespace SHLAPI.Controllers
{

    [Route("api/VoucherQabdSarf")]
    public class VoucherQabdSarfController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public VoucherQabdSarfController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("LoadQabdSarfScreen")]
        [HttpGet]
        public async Task<IActionResult> LoadQabdSarfScreen(GetLoadQabdScreenF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("VoucherQabdSarfGetData")]
        [HttpPost]
        public async Task<IActionResult> VoucherQabdSarfGetData([FromBody] VoucherQabdSarfGetDataF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetMaxVoucherNO")]
        [HttpGet]
        public async Task<IActionResult> GetMaxVoucherNO(GetMaxVoucherNOF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("CheckIfHaveManualVoucherNo")]
        [HttpGet]
        public async Task<IActionResult> CheckIfHaveManualVoucherNo(CheckIfHaveManualVoucherNoF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("SearchVouchers")]
        [HttpGet]
        public async Task<IActionResult> SearchVouchers(GetSearchVouchersF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("NavigateVouchers")]
        [HttpGet]
        public async Task<IActionResult> NavigateVouchers(NavigateVouchersF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("CheckVoucherHaveBillPay")]
        [HttpGet]
        public async Task<IActionResult> CheckVoucherHaveBillPay(CheckVoucherHaveBillPayF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("DeleteQabdSarfVoucher")]
        [HttpGet]
        public async Task<IActionResult> DeleteQabdSarfVoucher(DeleteQabdSarfVoucherF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("SaveQabdSarfVoucher")]
        [HttpPost]
        public async Task<IActionResult> SaveQabdSarfVoucher([FromBody] SaveQabdSarfVoucherF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
               [Route("CheckDateOfVoucher")]
        [HttpGet]
        public async Task<IActionResult> CheckDateOfVoucher(CheckDateOfVoucherF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        
    }
}
