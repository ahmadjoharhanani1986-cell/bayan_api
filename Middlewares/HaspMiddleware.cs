using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SHLAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using SHLAPI.Models.Authentication;
using SHLAPI.Database;

namespace SHLAPI.Middlewares
{
    public class HaspMiddleware
    {
        private readonly RequestDelegate _next;

        public HaspMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private Task Allow(HttpContext context)
        {
            return _next.Invoke(context);
        }

        private Task Reject(HttpContext context, string msg = null)
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.StatusCode = 503;

                return Task.CompletedTask;
            }, context);
            return context.Response.WriteAsync(JsonConvert.SerializeObject(string.Format(msg == null ? "The request has been rejected due licence issue" : msg)));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.ToLower().Contains("logout") || context.Request.Path.Value.ToLower().Contains("logout"))
            {
                await Allow(context);
                return;
            }
             bool isPlus = false;  bool isAuthenticated = false;    bool isDefualt = false;
            ShamelDatabase _con = new ShamelDatabase();
            using (var db = _con.Open())
            {
                isPlus = await Authentication_M.CheckHaspFatherIsPlus(db);
                isDefualt = await Authentication_M.CheckHaspFatherIsDefault(db);
               // isPlus = false;
                isAuthenticated = (isPlus && isDefualt) ? true : false;
            }
// #if DEBUG
//                 await Allow(context);
// #else
            if (isAuthenticated)
            {
                await Allow(context);
            }
            else
            {
                await Reject(context,(isPlus && isDefualt) == false ?"للاستفادة من المميزات الجديدة يرجى التواصل مع قسم المبيعات للحصول على بند الشامل لايت بلس":null );
            }
// #endif
        }


    }
}
