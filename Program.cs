using Microsoft.OpenApi.Models;

// 创建web应用构建器
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // 添加服务到容器

builder.Services.AddSwaggerGen(c => 
    {
        c.SwaggerDoc("api" ,new OpenApiInfo
        {
            Title = "运动场地预约系统 | 数据库网络应用程序接口 | Database Web API",
            Description = """
                          欢迎来到我们的运动场地预约系统。在这里你可以浏览我们的数据库网络应用程序。
                          """
        });
    }
);

// 构建web服务
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 启用开发者异常画面
}

app.UseSwagger(); // 启用swagger中间件
app.UseSwaggerUI(c => // 启用swaggerUI
    {
        c.SwaggerEndpoint("/swagger/api/swagger.json", "api");
        c.RoutePrefix = string.Empty; 
        c.DocumentTitle = "运动场地预约系统 - API";
    }
);

app.UseHttpsRedirection(); // 启动HTTPS重定向中间件 
app.MapControllers(); // 将控制器映射到路由
app.Run(); // 启动应用程序并开始处理请求
