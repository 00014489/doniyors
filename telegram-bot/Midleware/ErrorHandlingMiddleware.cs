using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace telegram_bot.Midleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                context.Response.StatusCode = 500;
            }
        }
    }
}