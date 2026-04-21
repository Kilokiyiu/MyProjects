using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace MiniWeb_Middleware;

/// <summary>
/// This is a Middleware for processing static files in website
/// Responsible for interception HTTP request and determining whether the request is a static file(such as HTML, CSS, js)
/// if so,directly return the contents of the file, otherwise it will be handed over to the next middleware for processing
/// </summary>

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
        //Get the request path from HTTP request
        string path = httpContext.Request.Path.Value ?? ""; //'?? ""' is a null-coalescing operator,if the Value is empty, then use the empty string "" as the default value
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

    //This method is responsible for copying the file stream to the memory stream,then converting it to byte[]
    private static async Task<byte[]> ToArrayAsync(Stream stream)
    {
        using MemoryStream memStream = new MemoryStream();
        await stream.CopyToAsync(memStream);
        memStream.Position = 0;
        byte[] bytes = memStream.ToArray();
        return bytes;
    }
}