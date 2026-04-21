namespace Markdown_Middleware;

/*
 * 我们开发的中间件时构建在ASP.NET CORE内置的StaticFiles中间件上的的，并且在他运行之前，所有的.md文件都被放在了wwwroot文件夹下，当请求wwwroot文件夹下的其他静态文件的时候，staticFiles
 * 中间件会把他们返回给浏览器，而当我们请求wwwroot下的.md文件时，编写的中间件会读取的对应的.md我呢见并且把他们装欢成HTML格式返回给浏览器
 *
 * What is StaticFiles?
 * StaticFiles middleware is ASP.NET Core's built-in middleware is used to
 * directly provide static files (HTML, CSS, JS, images, fonts, etc.) to the client without going through the MVC controller or Razor page processing.
 */

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();
        
        app.UseMiddleware<MarkdownMiddleware>();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
