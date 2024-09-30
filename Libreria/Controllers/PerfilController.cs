using Libreria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace Libreria.Controllers
{
    public class PerfilController : Controller
    {
        private readonly string _connectionString;

        public PerfilController(IConfiguration configuration)
        {
            // Obtener la cadena de conexión desde la configuración
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción Perfil: Muestra los detalles del usuario que ha iniciado sesión
        public IActionResult Perfil()
        {
            string userEmail = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = ObtenerUsuarioPorEmail(userEmail);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // Acción EditarPerfil: Muestra el formulario de edición del usuario
        [HttpGet]
        public IActionResult EditarPerfil()
        {
            string userEmail = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = ObtenerUsuarioPorEmail(userEmail);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // Acción EditarPerfil (POST): Actualiza los datos del usuario, incluyendo el email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(Usuario model)
        {
            // Asegurarse de que los datos recibidos del formulario son válidos
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userEmail = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioExistente = ObtenerUsuarioPorEmail(userEmail);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Actualizar los campos del usuario existente con los nuevos valores del modelo
            usuarioExistente.Nombre = model.Nombre;
            usuarioExistente.Apellidos = model.Apellidos;
            usuarioExistente.Telefono = model.Telefono;
            usuarioExistente.FechaNacimiento = model.FechaNacimiento;
            usuarioExistente.Genero = model.Genero;

            // Validar si el email ha cambiado
            if (model.Email != usuarioExistente.Email)
            {
                if (EmailYaRegistrado(model.Email))
                {
                    ModelState.AddModelError("Email", "El email ya está registrado por otro usuario.");
                    return View(model);
                }
                else
                {
                    usuarioExistente.Email = model.Email;
                }
            }

            // Guardar los cambios en la base de datos
            try
            {
                ActualizarUsuario(usuarioExistente);
                // Actualizar el email del usuario autenticado en el contexto de la sesión
                ActualizarEmailUsuarioSesion(usuarioExistente.Email);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar los datos: {ex.Message}");
                return View(model);
            }

            return RedirectToAction("Perfil");
        }

        // Método para actualizar el email en la sesión del usuario autenticado
        private void ActualizarEmailUsuarioSesion(string nuevoEmail)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var emailClaim = identity.FindFirst(ClaimTypes.Name);

            if (emailClaim != null)
            {
                identity.RemoveClaim(emailClaim);
                identity.AddClaim(new Claim(ClaimTypes.Name, nuevoEmail));
            }
        }

        // Método privado para verificar si un email ya está registrado
        private bool EmailYaRegistrado(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"SELECT COUNT(1) FROM Usuarios WHERE Email = @Email";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
        }

        // Método privado para actualizar los datos del usuario en la base de datos
        private void ActualizarUsuario(Usuario usuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"UPDATE Usuarios
                              SET Nombre = @Nombre,
                                  Apellidos = @Apellidos,
                                  Telefono = @Telefono,
                                  FechaNacimiento = @FechaNacimiento,
                                  Genero = @Genero,
                                  Email = @Email
                              WHERE UsuarioID = @UsuarioID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@Apellidos", usuario.Apellidos);
                    command.Parameters.AddWithValue("@Telefono", (object)usuario.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FechaNacimiento", (object)usuario.FechaNacimiento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Genero", usuario.Genero);
                    command.Parameters.AddWithValue("@Email", usuario.Email);
                    command.Parameters.AddWithValue("@UsuarioID", usuario.UsuarioID);

                    int affectedRows = command.ExecuteNonQuery();

                    // Verificar si se actualizó algún registro
                    if (affectedRows == 0)
                    {
                        throw new Exception("No se pudo actualizar el usuario.");
                    }

                    // Log de depuración para ver las filas afectadas
                    Console.WriteLine($"Filas actualizadas: {affectedRows}");
                }
            }
        }

        // Método privado para obtener un usuario por su email
        private Usuario ObtenerUsuarioPorEmail(string email)
        {
            Usuario usuario = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"SELECT UsuarioID, Nombre, Apellidos, Email, Contraseña, Telefono, FechaNacimiento, Genero, Rol 
                              FROM Usuarios 
                              WHERE Email = @Email";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                UsuarioID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellidos = reader.GetString(2),
                                Email = reader.GetString(3),
                                Contraseña = reader.GetString(4),
                                Telefono = reader.IsDBNull(5) ? null : reader.GetString(5),
                                FechaNacimiento = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                Genero = reader.GetString(7),
                                Rol = reader.GetString(8)
                            };
                        }
                    }
                }
            }

            return usuario;
        }
    }
}
