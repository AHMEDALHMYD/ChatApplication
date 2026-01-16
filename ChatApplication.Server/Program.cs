using ChatApplication.Server.Data;
using ChatApplication.Server.Hubs;
using ChatApplication.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ✅ SQLite DbContext
builder.Services.AddDbContext<ChatDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=chat.db";

    options.UseSqlite(cs);
});

// ✅ Auth Service DI
builder.Services.AddScoped<AuthService>();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ JWT Authentication (إذا عندك توكن)
var secretKey = builder.Configuration["Authentication:Secretkey"];
if (!string.IsNullOrWhiteSpace(secretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = builder.Configuration["Authentication:Issuer"],
                ValidAudience = builder.Configuration["Authentication:Audience"],
                ValidateLifetime = true
            };
        });
}

// ✅ CORS (Vercel)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                !string.IsNullOrWhiteSpace(origin) &&
                origin.StartsWith("https://chat-application") &&
                origin.EndsWith(".vercel.app")
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ✅ Forwarded headers (Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// ✅ إنشاء/تطبيق المايغريشن عند الإقلاع
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// ✅ Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors("CorsPolicy");

// لازم يكون قبل Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
