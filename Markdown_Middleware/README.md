# Markdown_Middleware

This is a based on ASP.NET CORE's cunstom middleware project. It could automatically convert the Markdown file in 'wwwroot' directory into an HTML page in real time, and return to your browser.

## Features

- **Automatically convert Markdown file into Html file**：In the target directory，the middleware automatically read files and convert it into HTML file
- **Automatic coding detection**：Using `Ude.NetStandard` auto detect file encoding(support UTF-8)，avoid garbled code
- **Seamless integration with StaticFiles**：Based on ASP.NET Core is built with StaticFiles middleware, and non Markdown requests normally go through the static file pipline
- **Support MVC**：Retains full ASP.NET Core MVC functionality (controllers, views, routes, etc.)


## Project structure

```
Markdown_Middleware/
├── Markdown_Middleware/
│   ├── Controllers/
│   │   └── HomeController.cs          
│   ├── Models/
│   │   └── ErrorViewModel.cs          
│   ├── Views/                          
│   ├── wwwroot/                        
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── testMd.md                   
│   ├── Program.cs                      
│   ├── MarkdownMiddleware.cs           
│   ├── appsettings.json
│   └── Markdown_Middleware.csproj
└── Markdown_Middleware.sln
```

## Quick start

### 1. Clone and enter the project

```bash
cd Markdown_Middleware
```

### 2. Access Markdown files

Put the Markdown files into `wwwroot/` directory，and access it in your browser：

```
https://your localhost/testMd.md
```




## core implementation

### MarkdownMiddleware Working principle

1. Intercept all HTTP requests and determine whether the path ends with `.md`
2. If it is not a Markdown request, it will be passed to the next middleware
3. If it is a Markdown request, read the file from `WebRootFileProvider`
4. Use `Ude.CharsetDetector` to automatically detect file encoding
5. Convert Markdown text to HTML using `MarkdownSharp`
6. Set the response header `Content-Type: text/html; charset=utf-8` and return HTML content

## 

| The NugetPackagse you have to intall | version |                              |
| ------------------------------------ | ------- | ---------------------------- |
| Ude.NetStandard                      | 1.2.0   | Character encoding detection |
| MarkdownSharp                        | 2.0.5   | Markdown convert into HTML   |
|                                      |         |                              |

