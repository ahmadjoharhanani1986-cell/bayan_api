using System;
using SHLAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SHLAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public static User CurrentUser(this IHttpContextAccessor context)
        {
            return context.HttpContext.Items["CurrentUser"] as User;
        }

        public static User CurrentUser(this HttpContext context)
        {
            return context.Items["CurrentUser"] as User;
        }

        public static User CurrentUser(this ActionExecutingContext context)
        {
            return context.HttpContext.Items["CurrentUser"] as User;
        }
    }
}