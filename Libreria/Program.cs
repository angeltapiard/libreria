using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configura el servicio de sesi�n
builder.Services.AddDistributedMemoryCache(); // Necesario para usar la sesi�n en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true; // Asegura que la cookie de sesi�n no sea accesible por el lado del cliente (JavaScript)
    options.Cookie.IsEssential = true; // Marca la cookie de sesi�n como esencial
});

// Configura los servicios de autenticaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Iniciar"; // Ruta a la que se redirige si no est� autenticado
        options.AccessDeniedPath = "/Login/AccessDenied"; // Ruta a la que se redirige si no tiene acceso
        options.SlidingExpiration = true; // Activa la expiraci�n deslizante
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de expiraci�n de la autenticaci�n
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Manejador de excepciones para producci�n
    app.UseHsts(); // Habilita el encabezado HSTS
}

app.UseHttpsRedirection(); // Redirige las solicitudes HTTP a HTTPS
app.UseStaticFiles(); // Permite el acceso a archivos est�ticos

app.UseRouting(); // Habilita el enrutamiento

// Usa la sesi�n antes de la autorizaci�n
app.UseSession(); // Activa la funcionalidad de sesi�n

app.UseAuthentication(); // Activa la autenticaci�n
app.UseAuthorization(); // Activa la autorizaci�n

// Configura las rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicia la aplicaci�n
app.Run();
