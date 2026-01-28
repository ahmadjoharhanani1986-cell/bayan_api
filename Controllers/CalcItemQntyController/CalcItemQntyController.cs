
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.CalcItemQnty;

namespace SHLAPI.Controllers
{

    [Route("api/CalcItemQnty")]
    public class CalcItemQntyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public CalcItemQntyController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetCalcItemQnty")]
        [HttpGet]
        public async Task<IActionResult> GetCalcItemQnty(GetCalcItemQntyF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetDataByExpiryDate")]
        [HttpPost]
        public async Task<IActionResult> GetDataByExpiryDate([FromBody]GetDataByExpiryDateF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
    }
}
