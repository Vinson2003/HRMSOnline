using HRMS.Components;
using HRMS.Data.Context;
using HRMS.Data.Entity;
using HRMS.Service.Interface;
using HRMS.Service.Service;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Register the database context with SQL Server.
builder.Services.AddDbContext<HRMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services using scoped dependency injection.
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IPasswordHasher<MstUser>, PasswordHasher<MstUser>>();

// Configure cookie-based authentication and session settings.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/access-denied";

        options.Cookie.Name = "HRMSCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Register authorization services and cascading authentication state.
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Seed demo accounts (only creates them if they do not exist yet)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HRMSContext>();
    var demoPassword = app.Configuration["Seed:DemoPassword"];

    await DbSeeder.SeedAsync(context, demoPassword);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Enable antiforgery protection for supported requests.
app.UseAntiforgery();

// Set Login endpoint
app.MapPost("/account/login", async (HttpContext context, IAuthService authService, IAntiforgery antiforgery) =>
{
    // Validate antiforgery token.
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest("Invalid antiforgery token.");
    }

    var form = await context.Request.ReadFormAsync();

    var username = form["Username"].ToString();
    var password = form["Password"].ToString();

    // Validate credentials through AuthService.
    var response = await authService.AuthenticateAsync(username, password);

    if (response.Result == null)
    {
        return Results.LocalRedirect("/login?error=invalid");
    }

    var user = response.Result;

    // Create authentication claims.
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new("EmployeeId", user.EmployeeId.ToString())
    };

    if (!string.IsNullOrWhiteSpace(user.RoleName))
    {
        claims.Add(new Claim(ClaimTypes.Role, user.RoleName));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    var principal = new ClaimsPrincipal(identity);

    // Create authentication cookie.
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties
        {
            IsPersistent = false
        }
    );

    return Results.LocalRedirect("/");
})
.AllowAnonymous();

// Set logout endpoint
app.MapPost("/account/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest("Invalid antiforgery token.");
    }

    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.LocalRedirect("/login");

}).RequireAuthorization();

// Map static assets and configure the Blazor Interactive Server application.
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
