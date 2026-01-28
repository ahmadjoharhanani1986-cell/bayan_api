
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;

namespace SHLAPI.Controllers
{
    [Route("api/Lookup")]
    public class LookupController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _context;
        public LookupController(IMediator mediator, IHttpContextAccessor context)
        {
            _mediator = mediator;
            _context = context;
        }

        [Route("GetPriorties")]
        [HttpGet]
        public async Task<IActionResult> GetPriorties(PrioritiesQueryF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeletePriorty")]
        [HttpGet]
        public async Task<IActionResult> DeletePriorty(DeleteLookupF.Command cmd)
        {
            cmd.tableName="priority_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SavePriorty")]
        [HttpPost]
        public async Task<IActionResult> SavePriorty([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="priority_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("GetClassifications")]
        [HttpGet]
        public async Task<IActionResult> GetClassifications(LookupsQueryF.Query qry)
        {
            qry.tableName="stories_classifications_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeleteClassification")]
        [HttpGet]
        public async Task<IActionResult> DeleteClassification(DeleteLookupF.Command cmd)
        {
            cmd.tableName="stories_classifications_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SaveClassification")]
        [HttpPost]
        public async Task<IActionResult> SaveClassification([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="stories_classifications_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }        


        [Route("GetTaskClassifications")]
        [HttpGet]
        public async Task<IActionResult> GetTaskClassifications(LookupsQueryF.Query qry)
        {
            qry.tableName="task_classifications_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeleteTaskClassification")]
        [HttpGet]
        public async Task<IActionResult> DeleteTaskClassification(DeleteLookupF.Command cmd)
        {
            cmd.tableName="task_classifications_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SaveTaskClassification")]
        [HttpPost]
        public async Task<IActionResult> SaveTaskClassification([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="task_classifications_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }


        [Route("GetTaskTypes")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTypes(LookupsQueryF.Query qry)
        {
            qry.tableName="tasks_types_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetTaskTypesForQuery")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTypesForQuery(GetTasksTypesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeleteTaskType")]
        [HttpGet]
        public async Task<IActionResult> DeleteTaskTypes(DeleteLookupF.Command cmd)
        {
            cmd.tableName="tasks_types_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SaveTaskType")]
        [HttpPost]
        public async Task<IActionResult> SaveTaskType([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="tasks_types_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("GetTestcaseType")]
        [HttpGet]
        public async Task<IActionResult> GetTestcaseType(LookupsQueryF.Query qry)
        {
            qry.tableName="testcases_types_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeleteTestcaseType")]
        [HttpGet]
        public async Task<IActionResult> DeleteTestcaseType(DeleteLookupF.Command cmd)
        {
            cmd.tableName="testcases_types_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SaveTestcaseType")]
        [HttpPost]
        public async Task<IActionResult> SaveTestcaseType([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="testcases_types_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("GetSeverity")]
        [HttpGet]
        public async Task<IActionResult> GetSeverity(LookupsQueryF.Query qry)
        {
            qry.tableName="severities_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("DeleteSeverity")]
        [HttpGet]
        public async Task<IActionResult> DeleteSeverity(DeleteLookupF.Command cmd)
        {
            cmd.tableName="severities_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [Route("SaveSeverity")]
        [HttpPost]
        public async Task<IActionResult> SaveSeverity([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="severities_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }        

        [Route("GetTaskStatuses")]
        [HttpGet]
        public async Task<IActionResult> GetTaskStatuses(LookupsQueryF.Query qry)
        {
            qry.tableName="tasks_types_statuses_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetRoles")]
        [HttpGet]
        public async Task<IActionResult> GetRoles(LookupsQueryF.Query qry)
        {
            qry.tableName="roles_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetTestcasesStatuses")]
        [HttpGet]
        public async Task<IActionResult> GetTestcasesStatuses(LookupsQueryF.Query qry)
        {
            qry.tableName="progressivity_statuses_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetEmployees")]
        [HttpGet]
        public async Task<IActionResult> GetEmployees(GetEmployeesF.Query qry)
        {
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("GetTestcaseTemplateTypes")]
        [HttpGet]
        public async Task<IActionResult> GetTestcaseTemplateTypes(LookupsQueryF.Query qry)
        {
            qry.tableName="testcase_template_types_tbl";
            Common.FillDefault(qry, _context);
            var result = await _mediator.Send(qry);
            return Ok(result);
        }

        [Route("SaveEmployee")]
        [HttpPost]
        public async Task<IActionResult> SaveEmployee([FromBody] SaveLookupF.Command cmd)
        {
            cmd.tableName="employees_tbl";
            Common.FillDefault(cmd, _context);
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
    }
}
