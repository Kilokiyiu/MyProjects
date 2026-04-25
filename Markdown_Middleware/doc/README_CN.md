# Markdown_Middleware

一个基于 ASP.NET Core 的自定义中间件项目，能够自动将 `wwwroot` 目录下的 `.md` Markdown 文件实时转换为 HTML 页面返回给浏览器。

## 功能特性

- **自动 Markdown 转 HTML**：请求以 `.md` 结尾的文件时，中间件自动读取并转换为 HTML
- **自动编码检测**：使用 `Ude.NetStandard` 自动检测文件编码（支持 UTF-8、GBK 等），避免中文乱码
- **无缝集成 StaticFiles**：基于 ASP.NET Core 内置 StaticFiles 中间件构建，非 Markdown 请求正常走静态文件管道
- **MVC 支持**：保留完整的 ASP.NET Core MVC 功能（控制器、视图、路由等）

## 技术栈

- **.NET 10**
- **ASP.NET Core MVC**
- **MarkdownSharp** — Markdown 解析
- **Ude.NetStandard** — 字符编码自动检测

## 项目结构

```
Markdown_Middleware/
├── Markdown_Middleware/
│   ├── Controllers/
│   │   └── HomeController.cs          # 首页、隐私页、错误页控制器
│   ├── Models/
│   │   └── ErrorViewModel.cs          # 错误视图模型
│   ├── Views/                          # Razor 视图
│   ├── wwwroot/                        # 静态文件根目录
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── testMd.md                   # 示例 Markdown 文件
│   ├── Program.cs                      # 应用入口 & 中间件管道配置
│   ├── MarkdownMiddleware.cs           # 核心：Markdown 转换中间件
│   ├── appsettings.json
│   └── Markdown_Middleware.csproj
└── Markdown_Middleware.sln
```

## 快速开始

### 1. 克隆并进入项目

```bash
cd Markdown_Middleware
```

### 2. 还原依赖并运行

```bash
dotnet restore
dotnet run --project Markdown_Middleware
```

### 3. 访问 Markdown 文件

将 `.md` 文件放入 `wwwroot/` 目录，然后在浏览器中直接访问：

```
https://localhost:5001/testMd.md
```

即可看到渲染后的 HTML 页面。

## 核心实现

### MarkdownMiddleware 工作原理

1. 拦截所有 HTTP 请求，判断路径是否以 `.md` 结尾
2. 若不是 Markdown 请求，则传递给下一个中间件
3. 若是 Markdown 请求，从 `WebRootFileProvider` 中读取文件
4. 使用 `Ude.CharsetDetector` 自动检测文件编码
5. 使用 `MarkdownSharp` 将 Markdown 文本转换为 HTML
6. 设置响应头 `Content-Type: text/html; charset=utf-8`，返回 HTML 内容

### 中间件注册顺序

```csharp
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// 自定义 Markdown 中间件（在 StaticFiles 之前）
app.UseMiddleware<MarkdownMiddleware>();

app.MapStaticAssets();
```

## 依赖包

| 包名 | 版本 | 说明 |
|------|------|------|
| MarkdownSharp | 2.0.5 | Markdown 转 HTML |
| Ude.NetStandard | 1.2.0 | 字符编码检测 |

## 许可证

MIT
