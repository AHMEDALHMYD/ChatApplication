using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS (الحل النهائي)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://chat-application-six-gilt.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
            // ❌ لا تضع AllowCredentials() طالما ما عم تستخدم Cookies
    });
});

// SignalR (إذا موجود عندك)
builder.Services.AddSignalR();

// إذا عندك Authentication / Authorization خليه مثل ما هو عندك
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// لو عندك Services / DbContext حطها هون (مثل مشروعك)
// builder.Services.AddDbContext<...>();
// builder.Services.AddScoped<...>();

var app = builder.Build();

// Forwarded headers (Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// ✅ لازم CORS يكون هون (بين UseRouting و UseAuthentication/MapControllers)
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// لو عندك ChatHub
app.MapHub<ChatHub>("/chatHub");

app.Run();
