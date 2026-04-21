using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using MiniWeb_Middleware;

namespace MiniWebAPI
{
    /// <summary>
    /// This is a middleware
    /// It is responsible for finding the corresponding Controller and action methods according to the request path,
    /// after that, call then through reflection,and serialize the results into JSON then return
    /// 流程如下：
    /// 请求进入 → 解析路径 → 查找 Action 方法 → 创建 Controller 实例 → 绑定参数 → 执行过滤器 → 反射调用方法 → 序列化结果 → JSON 响应
    /// </summary>
    public class MyWebAPIMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ActionLocator actionLocator;

        public MyWebAPIMiddleware(RequestDelegate next, ActionLocator actionLocator)
        {
            this.next = next;
            this.actionLocator = actionLocator;
        }
        
        public async Task InvokeAsync(HttpContext context, IServiceProvider sp)
        {
            //resolve path
            (bool ok, string? ctrlName, string? actionName)=PathParser.Parse(context.Request.Path);
            if(ok==false)
            {
                await next(context);
                return;
            }
            
            //Locator action method
            var actionMethod = actionLocator.LocateActionMethod(ctrlName!, actionName!);
            if(actionMethod ==null)
            {
                await next(context);
                return;
            }
            
            //Create a controller instance
            Type controllerType = actionMethod.DeclaringType!;
            object controllerInstance = sp.GetRequiredService(controllerType);
            var paraValues = BindingHelper.GetParameterValues(context, actionMethod);
            
            //Execute filter
            foreach(var filter in ActionFilter.Filters)
            {
                filter.Execute();
            }
            
            //Calling a method through reflection
            var result = actionMethod.Invoke(controllerInstance, paraValues);
            
            
            //The result is serialized and the response is written
            string jsonStr = JsonSerializer.Serialize(result);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(jsonStr);
        }
    }
}