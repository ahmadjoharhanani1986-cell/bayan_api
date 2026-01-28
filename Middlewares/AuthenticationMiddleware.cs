using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SHLAPI.Database;
using SHLAPI.Utilities;
using SHLAPI.Models;

using Task = System.Threading.Tasks.Task;
using System.Threading;
using Newtonsoft.Json;

namespace SHLAPI.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private IShamelDatabase _shamel;
        private static string[] WhitelistEndpoints = new[]
        {
            "api/login/userlogin",
            "/api/user/login",
            "/api/auth/",
            "/api/attachment/",
            "/api/samples/",
            "/api/dashboards"
        };

        public AuthenticationMiddleware(RequestDelegate next, IShamelDatabase shamel)
        {
            _next = next;
            _shamel = shamel;
        }

        private Task Allow(HttpContext context)
        {
            return _next.Invoke(context);
        }

        private Task Reject(HttpContext context)
        {
            context.Response.StatusCode = 401;
            return context.Response.WriteAsync(JsonConvert.SerializeObject("Not Authenticated"));
        }

        public async Task Invoke(HttpContext context)
        {
            if (WhitelistEndpoints.Any(ep => context.Request.Path.Value.ToLower().Contains(ep)))
            {
                await Allow(context);
                return;
            }
            string token = context.Request.Headers["token"];
            string user_id = context.Request.Headers["user_id"];
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(user_id))
            {
                await Reject(context);
                return;
            }
            int userId = int.Parse(context.Request.Headers["user_id"]);
            ShamelDatabase _con = new ShamelDatabase();
            using (var db = _con.Open())
            {
                UserM user = await UserM.Get(db,null,userId);
                if (user == null)
                {
                    await Reject(context);
                    return;
                }
                context.Items["CurrentUser"] = user;
                if(await UserM.IsTokenValid(db,null,user.id, token))
                {
                    await Allow(context);
                    return;
                }
                else
                {
                    await Reject(context);
                    return;
                }
            }


            // if (user != null)
            // {
            //     await Allow(context);
            // }
            // else
            // {
            //     await Reject(context);
            //     return;
            // }
        }
    }
}