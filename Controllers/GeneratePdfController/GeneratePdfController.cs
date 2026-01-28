
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SHLAPI.Features.GeneratePdf;

namespace SHLAPI.Controllers
{

    [Route("api/GeneratePdf")]
    public class GeneratePdfController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public GeneratePdfController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }
        [HttpPost("GetGeneratePdf")]
        public async Task<IActionResult> GetGeneratePdf([FromBody] GetGeneratePdfF.Command command)
        {
           // var result = await _mediator.Send(command);
        command.user_id = GetUserId(_context);
        var pdfGenerator = new GetGeneratePdfF();
        byte[] pdfBytes = await pdfGenerator.GeneratePdfAsync(command,command.user_id);
        // Return as file
        return File(pdfBytes, "application/pdf", "ItemsCatalog.pdf");
        }
              public static int GetUserId(IHttpContextAccessor _context)
        {
            int userId = 0;
            string ci = _context.HttpContext.Request.Headers["user_id"];
            int.TryParse(ci, out userId);
            return userId;
        }

    }
}
