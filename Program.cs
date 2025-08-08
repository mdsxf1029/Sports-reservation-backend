using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sports_reservation_backend.Data;
using Microsoft.Extensions.FileProviders;
using Sports_reservation_backend.Utils;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json")
    .Build();

// 创建web应用构建器
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var env = builder.Environment;

// 数据库
builder.Services.AddDbContext<OracleDbContext>(options =>
{
    var connectionString = "User Id="
                           + config["DatabaseConfig:UserId"]
                           + ";Password="
                           + config["DatabaseConfig:Password"]
                           + ";Data Source="
                           + config["DatabaseConfig:Host"]
                           + ":"
                           + config["DatabaseConfig:Port"]
                           + "/"
                           + config["DatabaseConfig:ServiceName"] + ";";
    options.UseOracle(connectionString, oracleOptions =>
    {
        // TODO 额外的oracle配置
    })
    .LogTo(Console.WriteLine, LogLevel.Information);
});

// JWT鉴权
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
    };
});

builder.Services.Configure<KestrelServerOptions>(options => { options.Limits.MaxRequestBodySize = null; });

builder.Services.AddControllers(); // 添加服务到容器

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("api", new OpenApiInfo
    {
        Title = "运动场地预约系统 | 数据库网络应用程序接口 | Database Web API",
        Description = "欢迎来到我们的运动场地预约系统。在这里你可以浏览我们的数据库网络应用程序。"
    });

    // JWT 支持
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT 授权，请输入: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 这一句启用文件上传支持
    c.OperationFilter<FileUploadOperationFilter>();
});


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

app.UseStaticFiles(); // 这行必须有

var wwwrootPath = Path.Combine(env.WebRootPath, "uploads");
    
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwrootPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowAll");
app.UseHttpsRedirection(); // 启动HTTPS重定向中间件 
app.UseAuthentication();   //  JWT 认证中间件（一定在 Authorization 之前）
app.UseAuthorization();    //  授权中间件
app.MapControllers();      // 将控制器映射到路由
app.Run();                 // 启动应用程序并开始处理请求
