using ActivosNetCore.Dependencias;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
//builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUtilitarios, Utilitarios>();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/CapturarError");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Activos}/{action=ListaActivos}/{id?}");

app.Run();
