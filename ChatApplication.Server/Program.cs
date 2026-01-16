using Microsoft.AspNetCore.HttpOverrides;
using ChatApplication.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// ✅ CORS (لازم سياسة واضحة)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy
            // ✅ اسمح لأي دومين من vercel.app + localhost
            .SetIsOriginAllowed(origin =>
                origin == "http://localhost:4200" ||
                (origin != null && origin.EndsWith(".vercel.app"))
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // مهم للـ SignalR / Cookies لو موجودة
    });
});

// (إذا عندك Auth/JWT services خليها مثل ما هي عندك)
// builder.Services.AddAuthentication(...);
// builder.Services.AddAuthorization(...);

var app = builder.Build();

// Forwarded headers (مفيد مع Render/Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ✅ ترتيب الميدلوير الصحيح
app.UseRouting();

// ✅ لازم يكون هون قبل auth
app.UseCors("AllowClient");

// بعد CORS
app.UseAuthentication();
app.UseAuthorization();

// ✅ فرض CORS على الـ Controllers والـ Hub
app.MapControllers().RequireCors("AllowClient");
app.MapHub<ChatHub>("/chatHub").RequireCors("AllowClient");

app.Run();
