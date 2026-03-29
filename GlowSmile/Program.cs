using GlowSmile.Data;
using GlowSmile.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة قاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. إضافة نظام الصلاحيات (Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login"; // المسار لو حد مش مسجل دخول
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // الجلسة تستمر 8 ساعات
    });

// 3. إضافة الخدمات (الترتيب هنا هو الحل)
builder.Services.AddTransient<EmailService>(); // ضفناها قبل الـ Build
builder.Services.AddRazorPages();

// --- هنا يتم غلق قائمة الخدمات وتحويلها إلى App جاهز للتشغيل ---
var app = builder.Build();

// 4. إعدادات الـ Middleware (بعد الـ Build)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// الترتيب هنا مهم جداً: الـ Authentication قبل الـ Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();