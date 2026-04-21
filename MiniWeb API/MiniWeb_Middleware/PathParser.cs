using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace MiniWebAPI
{
    public class PathParser
    {

        public static (bool ok,string? controllerName,string? actionName) Parse(PathString pathString)
        {
            string? path = pathString.Value;
            if (path == null)
            {
                return (false, null, null);
            }
            var match = Regex.Match(path,
                "/([a-zA-Z0-9]+)/([a-zA-Z0-9]+)");
            if (!match.Success)
            {
                return (false,null,null);
            }
            string controllerName = match.Groups[1].Value;
            string actionName = match.Groups[2].Value;
            return (true,controllerName,actionName);
        }
    }
}