using Microsoft.AspNetCore.Http.Features;
using schoolEnrollment.Utitlty;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
//builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.Configure<FormOptions>(option =>
{
    option.ValueLengthLimit = int.MaxValue;
    
    option.MultipartBodyLengthLimit = 1073741824;
});
builder.WebHost.ConfigureKestrel((context, options) =>
{
    
    options.Limits.MaxRequestBodySize = 1073741824;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainView}/{action=Index}/{id?}");

app.Run();
