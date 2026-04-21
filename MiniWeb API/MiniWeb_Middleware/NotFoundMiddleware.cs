using Microsoft.AspNetCore.Http;

namespace MiniWebAPI
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate next;

        public NotFoundMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html;charset=utf-8";
            await context.Response.WriteAsync("请求来到了一片未知的荒原");
        }
    }
}