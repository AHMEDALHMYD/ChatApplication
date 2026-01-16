using ChatApplication.Server.Data;
using ChatApplication.Server.Hubs;
using ChatApplication.Server.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS (حدد Origin تبعك بالضبط)
var allowedOrigins = new[]
{
    "https://chat-application-six-gilt.vercel.app"
    // إذا عندك دومين ثاني على vercel ضيفه هون كمان
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                !string.IsNullOrWhiteSpace(origin) &&
                (origin.EndsWith(".vercel.app") || origin.StartsWith("http://localhost:4200"))
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ✅ SQLite DbContext
builder.Services.AddDbContext<ChatDbContext>(options =>
{
    // لازم يكون SQLite connection string فقط
    var cs = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=chat.db";
    options.UseSqlite(cs);
});

// خدماتك
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Forwarded headers (Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ✅ مهم: UseRouting قبل CORS
app.UseRouting();

// ✅ مهم جداً: CORS مباشرة بعد UseRouting وقبل Auth
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// ✅ Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

app.Run();
