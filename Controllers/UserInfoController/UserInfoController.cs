
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.UserInfo;

namespace SHLAPI.Controllers
{

    [Route("api/UserInfo")]
    public class UserInfoController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public UserInfoController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetUserInfo")]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(GetUserInfoF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetUserDashboards")]
        [HttpGet]
        public async Task<IActionResult> GetUserDashboards(GetUserDashboardsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }
        [Route("GetUserProjectRights")]
        [HttpGet]
        public async Task<IActionResult> GetUserProjectRights(GetUserProjectRightsF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }


        
    }
}
