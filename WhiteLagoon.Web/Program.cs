using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//Add the custom DbContext child class
//Note: The AddDbContext method is used to register the DbContext class
builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>() //Add Identity User and Role to Identity
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//In general we don't have to add the following code because
//it is added by default and it will go to the action which is named
//Login and AccessDenied but we can provide our own custom
//Override of access denied and login path by using
//ConfigureApplicationCookie method
builder.Services.ConfigureApplicationCookie(option =>
{
    option.AccessDeniedPath = "/Account/AccessDenied";
    option.LoginPath = "/Account/Login";
});

//Configure the password complexity
//Override the default password complexity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); //Adding Dependency Injection
var app = builder.Build();
//Get the Stripe API Key from the appsettings.json file
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Use the Http Redirection Middleware to redirect HTTP requests to HTTPS
app.UseHttpsRedirection();
//Note: The UseStaticFiles method is used to serve static files
app.UseStaticFiles();

//Note: The UseRouting method is used to route the request to the appropriate endpoint
app.UseRouting();

//Note: The UseAuthentication method is used to authenticate the user
app.UseAuthorization();

//Note: The UseEndpoints method is used to map the endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
