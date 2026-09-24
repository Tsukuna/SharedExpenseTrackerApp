using Microsoft.EntityFrameworkCore;
using SharedExpenseTrackerApp.Database.AppDbContextModels;
using SharedExpenseTrackerApp.Domain.Features.Auth;
using SharedExpenseTrackerApp.Domain.Features.Expense;
using SharedExpenseTrackerApp.Domain.Features.ExpenseList;
using SharedExpenseTrackerApp.Domain.Features.Group;
using SharedExpenseTrackerApp.WebMvcApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ─── EF Core / Database ─────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Domain Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IExpenseListService, ExpenseListService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// ─── Session ────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes", 60));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// ─── SignalR ────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ─── MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ─── Pipeline ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ─── Routes ──────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// ─── SignalR Hub ──────────────────────────────────────────────────────────────
app.MapHub<ExpenseHub>("/hubs/expense");

app.Run();

