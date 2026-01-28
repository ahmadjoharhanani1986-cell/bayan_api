using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace SHLAPI.Infrastructure
{
    public class ValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                var result = new ContentResult();
                var errors = filterContext.ModelState
                .Where(m => m.Value.Errors.Count > 0)
                .Select(m => new
                {
                    m.Key,
                    Errors = m.Value.Errors.Select(e => new { e.ErrorMessage, Exception = e.Exception?.Message ?? "" })
                });
                string content = JsonConvert.SerializeObject(errors);
                Serilog.Log.Warning(content);
                result.Content = content;
                result.ContentType = "application/json";

                filterContext.HttpContext.Response.StatusCode = 400;
                filterContext.Result = result;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}