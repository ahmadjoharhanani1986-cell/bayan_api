using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using SHLAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SHLAPI.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();
            try
            {
                // await ValidateDictionary.InitializeValidateDictionary();
                sw.Start();
                await _next(context);
                sw.Stop();
                LogRequest(context, sw.ElapsedMilliseconds);
            }
            catch (PermissionException ex)
            {
                sw.Stop();
                await HandleExceptionAsync(context, ex, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                await HandleExceptionAsync(context, ex, sw.ElapsedMilliseconds);
            }
        }

        private void LogRequest(HttpContext context, long ms)
        {
            var currentUser = context.CurrentUser();
            var version = context.Request.Headers["VClient"];
            var user = currentUser == null ?  context.Request.Headers["user_id"].ToString() : currentUser.Id + "";
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            Serilog.Log.Information($"U{user} {version} {method} {path} {ms}ms");
        }

        private void LogException(HttpContext context, Exception exception, long ms)
        {
            var currentUser = context.CurrentUser();
            var version = context.Request.Headers["VClient"];
            var user = currentUser == null ? context.Request.Headers["user_id"].ToString() : currentUser.Id + "";
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            Serilog.Log.Error($"U{user} {version} {method} {path} {ms}ms " + exception.Demystify());
        }

        private Task HandleExceptionAsync(HttpContext context, PermissionException exception, long ms)
        {
            LogException(context, exception, ms);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            Result result = new Result();
            result.isSucceeded=false;
            result.mainError=new ErrorDescription(){id=(int)ErrorReason.NotAuthorized,description="You are not authorized to do this operation"};
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception, long ms)
        {
            LogException(context, exception, ms);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(exception.Message));
        }
    }
}