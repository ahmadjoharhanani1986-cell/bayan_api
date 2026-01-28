using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features.LogFile;

namespace SHLAPI.Controllers
{
  
     [Route("api/LogTrial")]
    public class LogTrialController : ControllerBase
    { 
       private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public LogTrialController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }
        
        [Route("GetRecordTransactionsLog")]
        [HttpGet]
        public async Task<IActionResult> GetRecordTransactionsLog(GetRecordTransactionsLogF.Query query)
        {
            query.language_id = Common.GetLanguageId(_context);
            query.page_id = Common.GetPageId(_context);
            query.user_id = Common.GetUserId(_context);
            var result = await _mediator.Send(query); 
            return Ok(result);
        }
    }
}
