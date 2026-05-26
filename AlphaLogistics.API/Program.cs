
using AlphaLogistics.API.Common;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));
builder.Services.AddScoped<IDashBoardService, DashBoardService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    }); builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// ✅ Only ONE AddSwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaLogistics API",
        Version = "v1",
        Description = "User Management API for AlphaLogistics",
        Contact = new OpenApiContact
        {
            Name = "AlphaLogistics Team",
            Email = "support@alphalogistics.com"
        }
    });
    // ✅ Add these to fix schema errors
    c.UseInlineDefinitionsForEnums();
    c.UseAllOfToExtendReferenceSchemas();
    c.CustomSchemaIds(type => type.FullName);
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.Domain = "";
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;  // ✅ Changed
        options.Cookie.SameSite = SameSiteMode.Lax;             // ✅ Changed
        options.LoginPath = "/api/User/Login";
        options.AccessDeniedPath = "/api/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("Vendor", policy =>
        policy.RequireRole("Vendor"));
});

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:4173",
                        "https://zoumapushpak.com:442",
                        "https://zoumapushpak.com:449",
                        "http://116.202.184.119",
                        "http://116.202.184.119:8080"
                    )
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// ✅ Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlphaLogistics API v1");
    c.RoutePrefix = "swagger";
});

// ✅ Correct middleware order
app.UseRouting();
app.UseCors("AllowSpecificOrigin");

// Upload directories
string[] uploadPaths = {
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "profiles"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "documents"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "products"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "payment")
};

foreach (var path in uploadPaths)
{
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
}

// Static files
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "profiles")),
    RequestPath = "/uploads/profiles"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "documents")),
    RequestPath = "/uploads/documents"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "products")),
    RequestPath = "/uploads/products"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "payment")),
    RequestPath = "/uploads/payment"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();



/*using AlphaLogistics.API.Common;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));
builder.Services.AddScoped<IDashBoardService, DashBoardService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaLogistics API",
        Version = "v1",
        Description = "User Management API for AlphaLogistics",
        Contact = new OpenApiContact
        {
            Name = "AlphaLogistics Team",
            Email = "support@alphalogistics.com"
        }
    });
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.Domain = "";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.LoginPath = "/api/User/Login";
        options.AccessDeniedPath = "/api/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("Admin", policy =>
        policy.RequireRole("SuperAdmin", "Admin"));

    options.AddPolicy("Vendor", policy =>
        policy.RequireRole("Vendor"));
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:4173",
                        "https://zoumapushpak.com:442",
                        "https://zoumapushpak.com:449",
                        "http://116.202.184.119",// User Portal
                        "http://116.202.184.119:8080"// Admin Portal
                    )
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowSpecificOrigin");
//app.UseHttpsRedirection();


// ? Create all upload directories if they don't exist (fixes Azure 500.30 startup crash)
string[] uploadPaths = {
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "profiles"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "documents"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "products"),
    Path.Combine(builder.Environment.ContentRootPath, "uploads", "payment")
};
  
foreach (var path in uploadPaths)
{
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
}

// Static files
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "profiles")),
    RequestPath = "/uploads/profiles"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "documents")),
    RequestPath = "/uploads/documents"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "products")),
    RequestPath = "/uploads/products"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads", "payment")),
    RequestPath = "/uploads/payment"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



*//*using AlphaLogistics.API.Common;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
*//*builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Error);

});*//*

builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));
builder.Services.AddScoped<IDashBoardService, DashBoardService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// Learn more about configuring Ope
//
// nAPI at https://aka.ms/aspnet/openapi

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaLogistics API",
        Version = "v1",
        Description = "User Management API for AlphaLogistics",
        Contact = new OpenApiContact
        {
            Name = "AlphaLogistics Team",
            Email = "support@alphalogistics.com"
        }
    });
});
// builder.Services.AddOpenApi(); // Remove or comment out this line

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.Domain = "";

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.LoginPath = "/api/User/Login";
        options.AccessDeniedPath = "/api/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

*//*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.Domain = ".zoumacorp.com";

        options.LoginPath = "/api/User/Login";
        options.AccessDeniedPath = "/api/User/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });
*//*
builder.Services.AddAuthorization(options =>
{
    // Add policies for different roles
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("Admin", policy =>
        policy.RequireRole("SuperAdmin", "Admin"));

    options.AddPolicy("Vendor", policy =>
        policy.RequireRole("Vendor"));

});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();


// Add CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(
                        "http://localhost:5173",   // dev
                        "http://localhost:4173",   // preview
                        "https://zoumapushpak.com:442"   // production frontend URL
                    )
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();
app.UseRouting();

// Before this line:
// app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(...) });

string uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads", "profiles");

// Add this - create the directory if it doesn't exist
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads/profiles"
});

app.UseStaticFiles();



app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads/profiles")),
    RequestPath = "/uploads/profiles"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads/documents")),
    RequestPath = "/uploads/documents"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads/products")),
    RequestPath = "/uploads/products"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads/payment")),
    RequestPath = "/uploads/payment"
});
app.MapControllers();

app.Run();
*/