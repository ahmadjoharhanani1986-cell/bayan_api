
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;

namespace SHLAPI.Controllers
{
    [Route("api/Login")]
    public class LoginController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public LoginController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("UserLogin")]
        [HttpPost]
        public async Task<IActionResult> UserLogin([FromBody]Authentication_F.Query cmd)
        {
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
    }
}
