
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;

namespace SHLAPI.Controllers
{

    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public UserController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("Save")]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveUserF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
        [Route("Update")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveUserF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("Get")]
        [HttpGet]
        public async Task<IActionResult> Get(GetUserF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.GetIt;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetPrevious")]
        [HttpGet]
        public async Task<IActionResult> GetPrevious(GetUserF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.Prev;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetNext")]
        [HttpGet]
        public async Task<IActionResult> GetNext(GetUserF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.Next;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetFirst")]
        [HttpGet]
        public async Task<IActionResult> GetFirst(GetUserF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.First;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetLast")]
        [HttpGet]
        public async Task<IActionResult> GetLast(GetUserF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.Last;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

/*
        [Route("ChangeStatus")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusProductF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }   */

        [Route("Delete")]
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteUserF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("Query")]
        [HttpPost]
        public async Task<IActionResult> Query([FromBody] UsersQueryF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("QueryIdName")]
        [HttpPost]
        public async Task<IActionResult> QueryIdName([FromBody] UsersQueryIdNameF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("UserRoles")]
        [HttpGet]
        public async Task<IActionResult> GetUserRoles(GetUserRolesListF.Query qry)
        {
            Common.FillDefault(qry, _context);
            qry.navigationType = NavigationTypes.GetIt;
            var result = await _mediator.Send(qry);
            return Ok(result);
        }


        [Route("UpdateLanguage")]
        [HttpGet]
        public async Task<IActionResult> UpdateLanguage(UpdateLanguageF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
        
        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordF.Command cmd)
        {
            Common.FillDefault(cmd, _context);
            //to comiit only
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }                
    }
}
