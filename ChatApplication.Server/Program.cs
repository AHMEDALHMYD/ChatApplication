using ChatApplication.Server.Hubs;
using ChatApplication.Server.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Controllers + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ✅ Swagger SERVICES (كان ناقص)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            // يسمح لأي رابط vercel من مشروعك
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("https://chat-application") && origin.EndsWith(".vercel.app")
                // (اختياري) لو عندك رابط ثاني مثل: https://chatapplication-six-gilt.vercel.app
                || origin.Contains(".vercel.app")
                // (اختياري) للاختبار محلياً
                || origin.StartsWith("http://localhost")
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ✅ DI
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// ✅ Forwarded headers (مناسب لـ Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// ✅ Swagger (خليه شغال حتى بالإنتاج)
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Routing ثم CORS
app.UseRouting();
app.UseCors("CorsPolicy");

// إذا أنت فعلياً مستخدم JWT/Identity خليهم
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
