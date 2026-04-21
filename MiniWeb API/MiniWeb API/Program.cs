using MiniWeb_Middleware;
using System.Reflection;
using MiniWeb_API;
using MiniWebAPI;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
ActionLocator locator = new ActionLocator(services, Assembly.GetEntryAssembly()!);
services.AddSingleton(locator);
services.AddMemoryCache();

ActionFilter.Filters.Add(new MyActionFilter.MyActionFilter1());
var app = builder.Build();

/*
 * 1.程序首先执行MyStaticFilesMiddleware这个中间件，用于确定请求的路径是否为静态文件路径，项目中可访问的静态文件路径为index.html，
 *   即：路径为http://localhost:5151/index.html时，web用于展现静态的index.html网页，如果不是，则进入下一个中间件
 * 2.如果访问路径不是静态文件路径，则进入MyWebAPIMiddleware这个中间件，这个中间件下有多个逻辑用于解析访问路径，并进行action匹配，如果
 *   请求不能再MyWebAPIMiddleware这个中间件执行，则进入NotFoundMiddleware中间件执行
 * 3.在前两个中间件都无法执行的情况下，表明请求不支持实现，返回对应状态码
 */

app.UseMiddleware<MyStaticFilesMiddleware>();
app.UseMiddleware<MyWebAPIMiddleware>();
app.UseMiddleware<NotFoundMiddleware>();

app.Run();