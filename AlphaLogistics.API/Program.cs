using AlphaLogistics.API.Common;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Error);

});*/

builder.Services.AddDbContext<AlphaLogisticsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
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
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/api/User/Login";
        options.AccessDeniedPath = "/api/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    // Add policies for different roles
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("Super Admin"));

    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Super Admin", "Admin"));

    options.AddPolicy("Vendor", policy =>
        policy.RequireRole("Vendor"));

});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();


// Add CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
               //.AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/Images"
});

app.MapControllers();

app.Run();
