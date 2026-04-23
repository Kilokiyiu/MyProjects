# MiniWeb API 原理

## 一、项目概述

**MiniWeb API** 是一个简化版 ASP.NET Core Web API 框架实现，通过手写自定义中间件来模拟 MVC 的核心机制。它剥离了 ASP.NET Core 的复杂封装，用最简洁的代码展示 Web 框架最核心的处理流程，非常适合用于学习 ASP.NET Core 底层原理。

### 项目结构

```
MiniWeb API/
├── MiniWeb API/              # 入口项目（Web 应用）
│   ├── Program.cs            # 服务注册与中间件管道配置
├── MiniWeb_Middleware/       # 核心类库（自定义中间件）
│   ├── MyWebApiMiddleware.cs      # API 请求核心处理中间件
│   ├── ActionLocator.cs           # 控制器扫描与 Action 定位
│   ├── PathParser.cs              # URL 路由解析
│   ├── BindingHelper.cs           # 请求参数绑定
│   ├── MyStaticFilesMiddleware.cs # 静态文件服务中间件
│   ├── ContentTypeHelper.cs       # 文件 Content-Type 映射
│   ├── NotFoundMiddleware.cs      # 404 兜底中间件
│   └── ActionFilter.cs            # 简易 Action 过滤器
```

---

## 二、核心原理：中间件管道（Middleware Pipeline）

### 2.1 什么是中间件管道？

ASP.NET Core 的请求处理本质是**管道模式（Pipeline Pattern）**。每个 HTTP 请求进入应用后，会按照注册顺序依次穿过多个中间件。每个中间件可以：

- **处理请求并短路**：直接返回响应，不再传递给后续中间件
- **传递给下一个**：自己不做处理，交给下一个中间件
- **先处理再传递**：执行某些前置逻辑，然后传递，等后续中间件处理完后再执行后置逻辑

### 2.2 本项目的管道配置

在 `Program.cs` 中，管道配置如下：

```csharp
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// 1. 扫描并注册所有控制器
ActionLocator locator = new ActionLocator(services, Assembly.GetEntryAssembly()!);
services.AddSingleton(locator);
services.AddMemoryCache();

var app = builder.Build();

// 2. 配置中间件管道（执行顺序从上往下）
app.UseMiddleware<MyStaticFilesMiddleware>();  // 第一层：静态文件服务
app.UseMiddleware<MyWebAPIMiddleware>();        // 第二层：API 请求处理
app.UseMiddleware<NotFoundMiddleware>();        // 第三层：404 兜底
```

### 2.3 请求处理流程图

```
HTTP 请求
    │
    ▼
┌─────────────────────────┐
│ MyStaticFilesMiddleware │ ──匹配静态文件？──→ 直接返回文件内容
└─────────────────────────┘         │否
                                    ▼
┌─────────────────────────┐
│   MyWebAPIMiddleware    │ ──匹配控制器Action？──→ 执行并返回JSON
└─────────────────────────┘              │否
                                         ▼
┌─────────────────────────┐
│   NotFoundMiddleware    │ ──→ 返回 404 页面
└─────────────────────────┘
```

**关键设计**：每个中间件处理不了的就 `await next(context)` 传给下一个，形成责任链。最后的 `NotFoundMiddleware` 不调用 `next`，确保任何未匹配请求都能得到响应。

---

## 三、控制器扫描与定位 — ActionLocator

### 3.1 职责

`ActionLocator` 模拟了 ASP.NET Core 的**控制器发现机制**和**端点路由表构建**过程。

### 3.2 实现原理

#### 步骤 1：启动时扫描程序集

```csharp
public ActionLocator(IServiceCollection services, Assembly assemblyWeb)
{
    // 从程序集中筛选出所有控制器类
    var controllerTypes = assemblyWeb.GetTypes().Where(IsControllerType);
    
    foreach (Type ctrlType in controllerTypes)
    {
        // 将控制器注册到 DI 容器（每次请求创建新实例）
        services.AddScoped(ctrlType);
        
        // 提取控制器名称（去掉末尾的 "Controller"）
        int index = ctrlType.Name.LastIndexOf("Controller");
        string controllerName = ctrlType.Name.Substring(0, index);
        
        // 提取所有 public 实例方法
        var methods = ctrlType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            string actionName = method.Name;
            // 用 "控制器名.方法名" 作为 Key 存入字典
            data[$"{controllerName}.{actionName}"] = method;
        }
    }
}
```

#### 步骤 2：控制器识别规则

```csharp
private static bool IsControllerType(Type t)
{
    return t.IsClass           // 必须是类
        && !t.IsAbstract       // 不能是抽象类
        && t.Name.EndsWith("Controller");  // 类名必须以 Controller 结尾
}
```

#### 步骤 3：运行时快速定位

```csharp
public MethodInfo? LocateActionMethod(string controllerName, string actionName)
{
    string key = $"{controllerName}.{actionName}";
    data.TryGetValue(key, out MethodInfo? method);
    return method;
}
```

### 3.3 设计特点

| 特性 | 说明 |
|------|------|
| 扫描时机 | **应用启动时**一次性扫描，避免运行时反射带来的性能损耗 |
| 存储结构 | 使用 `Dictionary<string, MethodInfo>`，O(1) 时间复杂度查找 |
| 大小写敏感 | 使用 `StringComparer.OrdinalIgnoreCase`，支持大小写不敏感匹配 |
| DI 集成 | 自动将发现的控制器注册到依赖注入容器（Scoped 生命周期） |

---

## 四、路由解析 — PathParser

### 4.1 职责

从 HTTP 请求路径中提取**控制器名称**和**Action 名称**。

### 4.2 实现原理

本项目采用**固定格式的简化路由**：

```
/{controllerName}/{actionName}
```

例如：
- 请求路径 `/Test/Save` → `controllerName="Test"`, `actionName="Save"`
- 请求路径 `/User/GetList` → `controllerName="User"`, `actionName="GetList"`

### 4.3 代码实现

```csharp
public static (bool ok, string? controllerName, string? actionName) Parse(PathString pathString)
{
    string? path = pathString.Value;
    if (path == null)
    {
        return (false, null, null);
    }
    
    // 使用正则表达式匹配两段路径
    var match = Regex.Match(path, "/([a-zA-Z0-9]+)/([a-zA-Z0-9]+)");
    if (!match.Success)
    {
        return (false, null, null);
    }
    
    string controllerName = match.Groups[1].Value;
    string actionName = match.Groups[2].Value;
    return (true, controllerName, actionName);
}
```

### 4.4 与 ASP.NET Core 路由的对比

| 特性 | MiniWeb API | ASP.NET Core MVC |
|------|-------------|------------------|
| 路由模板 | 固定格式 `/{controller}/{action}` | 支持属性路由 `[Route("api/[controller]")]` |
| 参数绑定 | 不支持路由参数 | 支持 `{id}` 等路由参数 |
| HTTP 谓词 | 不区分 GET/POST/PUT/DELETE | 支持 `[HttpGet]`、`[HttpPost]` 等 |

---

## 五、参数绑定 — BindingHelper

### 5.1 职责

模拟 ASP.NET Core 的 **Model Binding** 机制，将 HTTP 请求中的数据转换为 Action 方法的参数。

### 5.2 绑定规则

本项目做了大幅简化，只支持三种参数场景：

| 参数数量 | 参数类型 | 绑定来源 | 处理方式 |
|---------|---------|---------|---------|
| 0 个 | - | - | 返回空数组 `new object?[0]` |
| 1 个 | `HttpContext` | 框架注入 | 直接传入当前请求上下文 |
| 1 个 | 其他自定义类型 | 请求 Body (JSON) | 从 Body 反序列化 JSON |

### 5.3 代码实现

```csharp
public static object?[] GetParameterValues(HttpContext httpContext, MethodInfo actionMethod)
{
    var parameters = actionMethod.GetParameters();
    
    // 规则1：无参数
    if (parameters.Length <= 0)
    {
        return new object?[0];
    }
    // 规则2：最多1个参数
    else if (parameters.Length > 1)
    {
        throw new Exception("Action参数只能为1或0");
    }
    
    // 规则3：参数是 HttpContext 类型
    if (parameters.Single().ParameterType == typeof(HttpContext))
    {
        return new object?[] { httpContext };
    }
    
    // 规则4：参数是自定义类型，从 JSON Body 绑定
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
    return new object?[1] { paramValue };
}
```

### 5.4 与 ASP.NET Core Model Binding 的对比

| 特性 | MiniWeb API | ASP.NET Core MVC |
|------|-------------|------------------|
| 绑定源 | 仅 JSON Body | Body、QueryString、Route、Form、Header |
| 参数数量 | 最多1个 | 无限制 |
| 复杂对象 | 简单 JSON 反序列化 | 完整的模型验证和嵌套绑定 |
| 类型转换 | 依赖 JsonSerializer | 内置类型转换器系统 |

---

## 六、API 核心中间件 — MyWebAPIMiddleware

### 6.1 职责

这是整个框架的**心脏**，负责完整的 API 请求生命周期管理。

### 6.2 处理流程

```
┌─────────────────────────────────────┐
│         MyWebAPIMiddleware          │
├─────────────────────────────────────┤
│ 1. 解析请求路径                     │
│    └─→ PathParser.Parse(path)       │
│       └─→ (ctrlName, actionName)    │
│                                     │
│ 2. 查找 Action 方法                 │
│    └─→ ActionLocator.LocateActionMethod()│
│       └─→ MethodInfo                │
│                                     │
│ 3. 创建控制器实例                   │
│    └─→ 从 DI 容器获取               │
│       └─→ controllerInstance        │
│                                     │
│ 4. 绑定请求参数                     │
│    └─→ BindingHelper.GetParameterValues()│
│       └─→ paraValues[]              │
│                                     │
│ 5. 执行过滤器                       │
│    └─→ ActionFilter.Filters         │
│                                     │
│ 6. 调用 Action 方法                 │
│    └─→ MethodInfo.Invoke()          │
│       └─→ result                    │
│                                     │
│ 7. 序列化响应                       │
│    └─→ JsonSerializer.Serialize()   │
│       └─→ 写入 Response.Body        │
└─────────────────────────────────────┘
```

### 6.3 代码详解

```csharp
public async Task InvokeAsync(HttpContext context, IServiceProvider sp)
{
    // === 步骤1：解析路径 ===
    //将请求路径（如/api/User/GetAll）解析为 Controller名（User）和Action 名（GetAll）。解析失败则交给下一个中间件。
    (bool ok, string? ctrlName, string? actionName) = PathParser.Parse(context.Request.Path);
    if (ok == false)
    {
        await next(context);  // 不匹配 API 路由，传给下一个中间件
        return;
    }
    
    // === 步骤2：定位 Action 方法 ===
    //通过反射查找匹配的 MethodInfo。找不到则交给下一个中间件。
    var actionMethod = actionLocator.LocateActionMethod(ctrlName!, actionName!);
    if (actionMethod == null)
    {
        await next(context);  // 找不到对应方法，传给下一个中间件
        return;
    }
    
    // === 步骤3：获取控制器实例（支持依赖注入）===
    //DeclaringType 获取方法所属的 Controller 类型。
//通过依赖注入容器（IServiceProvider）创建 Controller 实例，这样 Controller 构造函数中的依赖也会被自动注入。
    Type controllerType = actionMethod.DeclaringType!;
    object controllerInstance = sp.GetRequiredService(controllerType);
    
    // === 步骤4：参数绑定 ===
    var paraValues = BindingHelper.GetParameterValues(context, actionMethod);
    
    // === 步骤5：执行过滤器 ===
    foreach (var filter in ActionFilter.Filters)
    {
        filter.Execute();
    }
    
    // === 步骤6：通过反射调用方法 ===
    var result = actionMethod.Invoke(controllerInstance, paraValues);
    
    // === 步骤7：结果序列化并写入响应 ===
    string jsonStr = JsonSerializer.Serialize(result);
    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsync(jsonStr);
}
```

### 6.4 设计约定

本项目对控制器做了如下简化约定：

1. **控制器类名**以 `Controller` 结尾，去掉后缀即为控制器名
2. **所有 public 实例方法**都是 Action，方法名就是 Action 名
3. **请求路径**固定为 `/控制器名/方法名`
4. **不支持方法重载**
5. **不支持 `[HttpGet]` 等 HTTP 谓词绑定**
6. **最多支持1个参数**（`HttpContext` 或自定义 JSON 对象）
7. **返回值自动 JSON 序列化**，不支持 `IActionResult` 等类型

---

## 七、静态文件中间件 — MyStaticFilesMiddleware

### 7.1 职责

模拟 `app.UseStaticFiles()`，提供 `wwwroot` 目录下的静态文件服务。

### 7.2 实现原理

```csharp
public async Task InvokeAsync(HttpContext httpContext)
{
    string path = httpContext.Request.Path.Value ?? "";
    
    // 从 WebRootFileProvider（默认指向 wwwroot）获取文件信息
    var file = _hostEnv.WebRootFileProvider.GetFileInfo(path);
    
    // 检查文件是否存在且类型受支持
    if (!file.Exists || !ContentTypeHelper.IsValid(file))
    {
        await _next(httpContext);  // 不是静态文件请求，传给下一个中间件
        return;
    }
    
    // 设置响应头和状态码
    httpContext.Response.ContentType = ContentTypeHelper.GetContentType(file);
    httpContext.Response.StatusCode = 200;
    
    // 读取文件并写入响应流
    using var stream = file.CreateReadStream();
    byte[] bytes = await ToArrayAsync(stream);
    await httpContext.Response.Body.WriteAsync(bytes);
}
```

### 7.3 ContentTypeHelper

维护了一个简单的文件扩展名到 MIME 类型的映射表：

| 扩展名 | Content-Type |
|--------|-------------|
| `.html`, `.htm` | `text/html; charset=utf-8` |
| `.txt` | `text/plain; charset=utf-8` |
| `.jpg`, `.jpeg` | `image/jpeg` |
| `.png` | `image/png` |
| `.js` | `application/x-javascript; charset=utf-8` |
| `.css` | `text/css` |

---

## 八、404 兜底中间件 — NotFoundMiddleware

### 8.1 职责

当请求无法被管道中任何中间件处理时，返回友好的 404 响应。

### 8.2 为什么放在最后？

```csharp
app.UseMiddleware<MyStaticFilesMiddleware>();  // 第1个：尝试处理静态文件
app.UseMiddleware<MyWebAPIMiddleware>();        // 第2个：尝试处理 API 请求
app.UseMiddleware<NotFoundMiddleware>();        // 第3个：兜底处理
```

- 如果静态文件中间件能处理，直接返回，不会到达后面的中间件
- 如果 API 中间件能匹配控制器，执行后返回，也不会到达 404
- 如果都处理不了，最终由 `NotFoundMiddleware` 返回 404

### 8.3 实现

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // 注意：这个中间件不调用 next，它就是管道的终点
    context.Response.StatusCode = 404;
    context.Response.ContentType = "text/html;charset=utf-8";
    await context.Response.WriteAsync("请求来到了一片未知的荒原");
}
```

---

## 九、简易过滤器 — ActionFilter

### 9.1 职责

提供 Action 执行前后的扩展点，模拟 ASP.NET Core 的 Filter 管道。

### 9.2 实现

```csharp
// 过滤器接口
public interface IMyActionFilter
{
    void Execute();
}

// 全局过滤器列表
public class ActionFilter
{
    public static List<IMyActionFilter> Filters = new List<IMyActionFilter>();
}
```

### 9.3 使用方式

```csharp
// 在应用启动时注册过滤器
ActionFilter.Filters.Add(new LogFilter());
ActionFilter.Filters.Add(new AuthFilter());
```

### 9.4 与 ASP.NET Core Filter 的对比

| 特性 | MiniWeb API | ASP.NET Core |
|------|-------------|--------------|
| 执行时机 | 仅在 Action 前 | 支持 Action 前、后、结果处理等 |
| 作用范围 | 全局唯一列表 | 支持全局、控制器、Action 三级 |
| 异步支持 | 同步 | 支持异步 `OnActionExecutionAsync` |
| 短路能力 | 无 | 支持通过 `ActionExecutedContext` 短路 |

---

## 十、完整请求生命周期示例

以请求 `POST /User/Save` 为例，假设请求 Body 为 `{"name":"张三","age":25}`：

```
┌─────────────────────────────────────────────────────────┐
│  1. HTTP 请求到达：POST /User/Save                      │
│     Body: {"name":"张三","age":25}                     │
├─────────────────────────────────────────────────────────┤
│  2. MyStaticFilesMiddleware                             │
│     └─→ 检查 wwwroot/User/Save 文件是否存在？           │
│         └─→ 不存在，传递给下一个中间件                   │
├─────────────────────────────────────────────────────────┤
│  3. MyWebAPIMiddleware                                  │
│     ├─→ PathParser.Parse("/User/Save")                 │
│     │   └─→ ctrlName="User", actionName="Save"         │
│     ├─→ ActionLocator.LocateActionMethod("User","Save")│
│     │   └─→ 找到 UserController.Save() 的 MethodInfo   │
│     ├─→ sp.GetRequiredService(typeof(UserController))  │
│     │   └─→ 从 DI 容器创建 UserController 实例          │
│     ├─→ BindingHelper.GetParameterValues(...)          │
│     │   └─→ 反序列化 Body 为 User 对象                  │
│     ├─→ 执行 ActionFilter.Filters 中的所有过滤器       │
│     ├─→ method.Invoke(controller, [userObj])           │
│     │   └─→ 返回 {"success":true, "id": 123}           │
│     └─→ JsonSerializer.Serialize(result)               │
│         └─→ 写入 Response，Content-Type: application/json│
├─────────────────────────────────────────────────────────┤
│  4. 响应返回客户端                                       │
│     Status: 200 OK                                       │
│     Body: {"success":true,"id":123}                      │
└─────────────────────────────────────────────────────────┘
```

---

## 十一、与 ASP.NET Core 核心机制的对比总结

| 自制组件 | 对应 ASP.NET Core 机制 | 复杂度对比 |
|---------|----------------------|-----------|
| `ActionLocator` | 控制器发现 + Endpoint 路由表构建 | 简化版：启动时扫描，字典存储 |
| `PathParser` | 路由模板解析 + RouteMatcher | 简化版：固定格式正则匹配 |
| `BindingHelper` | Model Binding + Value Providers | 简化版：仅支持 JSON Body 和 HttpContext |
| `MyWebAPIMiddleware` | MVC 请求处理管道 + ActionInvoker | 简化版：直接反射调用，无结果过滤器 |
| `MyStaticFilesMiddleware` | `UseStaticFiles()` + `StaticFileMiddleware` | 简化版：基础文件读取 |
| `NotFoundMiddleware` | 404 处理 + StatusCodePagesMiddleware | 简化版：固定响应 |
| `ActionFilter` | Action Filter 管道 | 简化版：仅同步前置执行 |

---

## 十二、学习价值

这个项目虽然简化了很多功能，但完整展示了 ASP.NET Core Web 框架的**核心骨架**：

1. **中间件管道**如何串联请求处理流程
2. **依赖注入**如何在控制器解析中发挥作用
3. **反射**如何实现动态方法调用
4. **路由解析**如何将 URL 映射到方法
5. **参数绑定**如何将 HTTP 数据转换为方法参数
6. **结果输出**如何将对象序列化为 HTTP 响应

