using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configura el servicio de sesión
builder.Services.AddDistributedMemoryCache(); // Necesario para usar la sesión en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true; // Asegura que la cookie de sesión no sea accesible por el lado del cliente (JavaScript)
    options.Cookie.IsEssential = true; // Marca la cookie de sesión como esencial
});

// Configura los servicios de autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Iniciar"; // Ruta a la que se redirige si no está autenticado
        options.AccessDeniedPath = "/Login/AccessDenied"; // Ruta a la que se redirige si no tiene acceso
        options.SlidingExpiration = true; // Activa la expiración deslizante
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de expiración de la autenticación
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Manejador de excepciones para producción
    app.UseHsts(); // Habilita el encabezado HSTS
}

app.UseHttpsRedirection(); // Redirige las solicitudes HTTP a HTTPS
app.UseStaticFiles(); // Permite el acceso a archivos estáticos

app.UseRouting(); // Habilita el enrutamiento

// Usa la sesión antes de la autorización
app.UseSession(); // Activa la funcionalidad de sesión

app.UseAuthentication(); // Activa la autenticación
app.UseAuthorization(); // Activa la autorización

// Configura las rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicia la aplicación
app.Run();
