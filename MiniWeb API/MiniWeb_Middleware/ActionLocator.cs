using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MiniWeb_Middleware;

/// <summary>
/// This is an Action locator, which is responsible for scanning all Controller classes in the assembly when the application starts,
/// and establishing a lookup table (dictionary).
/// At runtime, it can quickly locate the corresponding MethodInfo based on the Controller name and Action name.
/// </summary>
public class ActionLocator
{
    private Dictionary<string, MethodInfo> data = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsControllerType(Type t)
    {
        return t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller");
    }

    public ActionLocator(IServiceCollection services, Assembly assemblyWeb)
    {
        var controllerTypes = assemblyWeb.GetTypes().Where(IsControllerType); //调用IsControllerType来从程序集扫描以Controller结尾的Controller类
        foreach (Type ctrlType in controllerTypes)
        {
            services.AddScoped(ctrlType);
            int index = ctrlType.Name.LastIndexOf("Controller");
            string controllerName = ctrlType.Name.Substring(0, index);
            var methods = ctrlType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                string actionName = method.Name;
                data[$"{controllerName}.{actionName}"] = method;
            }
        }
    }

    public MethodInfo? LocateActionMethod(string controllerName, string actionName)
    {
        string key = $"{controllerName}.{actionName}";
        data.TryGetValue(key, out MethodInfo? method);
        return method;
    }
}