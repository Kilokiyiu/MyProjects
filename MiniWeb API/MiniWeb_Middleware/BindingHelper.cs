using System.Reflection;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MiniWeb_Middleware;

public class BindingHelper
{
    /// <summary>
    /// This is a parameter binding helper class
    /// that is responsible for extracting data from HTTP requests and converting it to the parameter values required by the Action method.
    /// equivalent to ASP.NET A simplified version of Model Binding in Core.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="actionMethod"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static object?[] GetParameterValues(HttpContext httpContext, MethodInfo actionMethod)
    {
        var parameters = actionMethod.GetParameters();
        if (parameters.Length <= 0)
        {
            return new object?[0];
        }
        else if (parameters.Length > 1)
        {
            throw new Exception("Action参数只能为1或0");
        }

        if (parameters.Single().ParameterType == typeof(HttpContext))
        {
            return new object?[]{httpContext};
        }

        if (!httpContext.Request.HasJsonContentType())
        {
            throw new Exception("Action如果只有一个参数，则contentType必须是一个application/json");
        }

        if (httpContext.Request.ContentLength == 0)
        {
            return new object?[1] { null };
        }

        var reqStream = httpContext.Request.BodyReader.AsStream();

        Type paramType = parameters.Single().ParameterType;
        object? paramValue = JsonSerializer.Deserialize(reqStream, paramType);
        return new object?[1]{paramValue};

    }
}