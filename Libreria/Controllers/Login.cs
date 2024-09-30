using Libreria.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Libreria.Controllers
{
    public class Login : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string connectionString;

        public Login(IConfiguration configuration)
        {
            this.configuration = configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método para encriptar la contraseña usando SHA256
        public static string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Acción para mostrar la vista de inicio de sesión
        public IActionResult Iniciar()
        {
            return View();
        }

        // Acción para mostrar la vista de registro
        public IActionResult Registrar()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // Acción para registrar un nuevo usuario
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Registrar(Usuario usuario)
        {
            // Verificar que todos los campos requeridos estén completos
            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Apellidos) ||
                string.IsNullOrWhiteSpace(usuario.Email) ||
                string.IsNullOrWhiteSpace(usuario.Contraseña) ||
                string.IsNullOrWhiteSpace(usuario.Telefono) ||
                usuario.FechaNacimiento == DateTime.MinValue ||
                string.IsNullOrWhiteSpace(usuario.Genero))
            {
                ViewData["Incompleto"] = "Por favor completa todos los campos requeridos.";
                return View(usuario); // Regresar el modelo para pre-llenar el formulario
            }

            using SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            using SqlTransaction transaction = con.BeginTransaction(); // Usar transacción para garantizar consistencia

            try
            {
                SqlCommand cmd = new SqlCommand("RegistrarUsuario", con, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Apellidos", usuario.Apellidos);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Contraseña", EncryptPassword(usuario.Contraseña)); // Encriptar contraseña
                cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
                cmd.Parameters.AddWithValue("@FechaNacimiento", usuario.FechaNacimiento);
                cmd.Parameters.AddWithValue("@Genero", usuario.Genero);
                cmd.Parameters.AddWithValue("@Rol", "Cliente"); // Rol por defecto

                // Parámetro de salida para obtener el ID del usuario recién registrado
                SqlParameter usuarioIdParam = new SqlParameter("@UsuarioID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(usuarioIdParam);
                cmd.ExecuteNonQuery();

                int usuarioID = (int)usuarioIdParam.Value;

                // Crear un carrito para el usuario recién registrado
                SqlCommand carritoCmd = new SqlCommand("INSERT INTO Carrito (UsuarioID) VALUES (@UsuarioID)", con, transaction);
                carritoCmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                carritoCmd.ExecuteNonQuery();

                transaction.Commit();

                ViewData["Completado"] = "Registro Completado";
                return View();
            }
            catch (SqlException ex) when (ex.Number == 2627) // Código de error SQL para violación de restricción única
            {
                transaction.Rollback(); // Hacer rollback en caso de error
                ViewData["Incompleto"] = "El correo electrónico ya está en uso.";
                return View(usuario); // Regresar el modelo para pre-llenar el formulario
            }
            catch (Exception)
            {
                transaction.Rollback(); // Hacer rollback en caso de error
                ViewData["Incompleto"] = "Registro no completado, por favor intenta de nuevo.";
                return View(usuario); // Regresar el modelo para pre-llenar el formulario
            }
        }

        // Acción para iniciar sesión
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Iniciar(Usuario usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Email) || string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                ViewData["IncompletoInicio"] = "Por favor completa todos los campos requeridos.";
                return View("Iniciar");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Intentamos primero con la contraseña encriptada
                string encryptedPassword = EncryptPassword(usuario.Contraseña);

                SqlCommand command = new SqlCommand("ValidarUsuario", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@Contraseña", encryptedPassword);

                // Parámetro de salida para el resultado de la validación
                SqlParameter outputParameter = new SqlParameter("@Resultado", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(outputParameter);

                connection.Open();
                await command.ExecuteNonQueryAsync();

                bool esValido = (bool)outputParameter.Value;

                if (!esValido)
                {
                    // Si no es válido, intentamos con la contraseña sin encriptar
                    command.Parameters["@Contraseña"].Value = usuario.Contraseña; // Usamos la contraseña sin encriptar

                    await command.ExecuteNonQueryAsync();
                    esValido = (bool)outputParameter.Value;
                }

                if (esValido)
                {
                    // Obtener el ID del usuario
                    SqlCommand userIdCommand = new SqlCommand("SELECT UsuarioID FROM Usuarios WHERE Email = @Email", connection);
                    userIdCommand.Parameters.AddWithValue("@Email", usuario.Email);
                    int usuarioID = (int)await userIdCommand.ExecuteScalarAsync();

                    // Verificar si el usuario ya tiene un carrito
                    SqlCommand carritoCheckCommand = new SqlCommand("SELECT CarritoID FROM Carrito WHERE UsuarioID = @UsuarioID", connection);
                    carritoCheckCommand.Parameters.AddWithValue("@UsuarioID", usuarioID);
                    var carritoID = await carritoCheckCommand.ExecuteScalarAsync();

                    if (carritoID == null) // Si no tiene carrito, crearlo
                    {
                        SqlCommand carritoCreateCommand = new SqlCommand("INSERT INTO Carrito (UsuarioID) VALUES (@UsuarioID)", connection);
                        carritoCreateCommand.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        await carritoCreateCommand.ExecuteNonQueryAsync();
                    }

                    // Obtener el rol del usuario
                    SqlCommand roleCommand = new SqlCommand("ObtenerRolIDUsuario", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    roleCommand.Parameters.AddWithValue("@Email", usuario.Email);

                    SqlParameter roleIdParameter = new SqlParameter("@Rol", SqlDbType.NVarChar, 50)
                    {
                        Direction = ParameterDirection.Output
                    };

                    roleCommand.Parameters.Add(roleIdParameter);
                    await roleCommand.ExecuteNonQueryAsync();

                    string rolUsuario = (string)roleIdParameter.Value;

                    // Crear las claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Email),
                        new Claim(ClaimTypes.Role, rolUsuario),
                        new Claim("UsuarioID", usuarioID.ToString()) // Agregar el ID del usuario a las claims
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // Redirigir según el rol
                    if (rolUsuario == "Admin")
                        return RedirectToAction("Index", "Dashboard"); // Redirigir al dashboard de Admin
                    else
                        return RedirectToAction("Index", "Home"); // Redirigir a Home para clientes
                }

                // Credenciales inválidas
                ViewData["IncompletoInicio"] = "Usuario o contraseña incorrectos";
                return View("Iniciar");
            }
        }

        // Acción para cerrar sesión
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Cerrar sesión del usuario
            return RedirectToAction("Iniciar", "Login"); // Redirigir a la página de inicio de sesión después del logout
        }
    }
}
