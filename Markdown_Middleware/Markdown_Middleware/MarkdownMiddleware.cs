using System.Text;
using MarkdownSharp;

namespace Markdown_Middleware;

public class MarkdownMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _hostEnv;

    public MarkdownMiddleware(RequestDelegate next, IWebHostEnvironment hostEnv)
    {
        this._next = next;
        this._hostEnv = hostEnv;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path.ToString();
        if (!path.EndsWith(".md", true, null))
        {
            await _next(context);
            return;
        }
        var file = _hostEnv.WebRootFileProvider.GetFileInfo(path);
        if (!file.Exists)
        {
            await _next(context);
            return;
        }

        using var stream = file.CreateReadStream();
        Ude.CharsetDetector cdet = new Ude.CharsetDetector();
        cdet.Feed(stream);
        cdet.DataEnd();
        string charset = cdet.Charset??"utf-8";
        //Console.WriteLine($"[DEBUG] Ude detected charset: '{cdet.Charset}', using: '{charset}'");
        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(charset));
        string mdText = await reader.ReadToEndAsync();

        Markdown md = new Markdown();
        string html = md.Transform(mdText);
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
    }
}