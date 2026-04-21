using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace MiniWeb_Middleware;

public class MyStaticFilesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _hostEnv;

    public MyStaticFilesMiddleware(RequestDelegate httpContext, IWebHostEnvironment hostEnv)
    {
        this._next = httpContext;
        this._hostEnv =  hostEnv;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        string path = httpContext.Request.Path.Value ?? "";
        var file = _hostEnv.WebRootFileProvider.GetFileInfo(path);
        if (!file.Exists||!ContentTypeHelper.IsValid(file))
        {
            await _next(httpContext);
            return;
        }
        httpContext.Response.ContentType = ContentTypeHelper.GetContentType(file);
        httpContext.Response.StatusCode = 200;
        using var stream = file.CreateReadStream();
        byte[] bytes = await ToArrayAsync(stream);
        await httpContext.Response.Body.WriteAsync(bytes);
    }

    private static async Task<byte[]> ToArrayAsync(Stream stream)
    {
        using MemoryStream memStream = new MemoryStream();
        await stream.CopyToAsync(memStream);
        memStream.Position = 0;
        byte[] bytes = memStream.ToArray();
        return bytes;
    }
}