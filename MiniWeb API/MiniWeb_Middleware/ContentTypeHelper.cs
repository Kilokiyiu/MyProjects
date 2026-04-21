using Microsoft.Extensions.FileProviders;

namespace MiniWeb_Middleware;

public class ContentTypeHelper
{
    private static readonly Dictionary<string, string> data = new(StringComparer.OrdinalIgnoreCase);

    static ContentTypeHelper()
    {
        data[".html"] = "text/html; charset=utf-8";
        data[".htm"] = "text/html; charset=utf-8";
        data[".txt"] = "text/plain; charset=utf-8";
        data[".jpg"] = "image/jpeg";
        data[".jpeg"] = "image/jpeg";
        data[".png"] = "image/png";
        data[".js"] = "application/x-javascript; charset=utf-8";
        data[".css"] = "text/css";
    }

    public static bool IsValid(IFileInfo file)
    {
        if (file.IsDirectory)
        {
            return false;
        }

        string extension = Path.GetExtension(file.Name);
        return data.ContainsKey(extension);
    }

    public static string GetContentType(IFileInfo file)
    {
        string extentions = Path.GetExtension(file.Name);
        return data[extentions];
    }
}