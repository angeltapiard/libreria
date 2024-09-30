using Libreria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Libreria.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly string _connectionString;

        public UsuariosController(IConfiguration configuration)
        {
            // Obtener la cadena de conexión desde la configuración
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción Index: Muestra la lista de usuarios
        public IActionResult Index()
        {
            var usuarios = ObtenerTodosLosUsuarios();
            return View(usuarios);
        }

        // Acción Detalles: Muestra detalles de un usuario
        public IActionResult Detalles(int id)
        {
            var usuario = ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // Acción Editar: Cargar el formulario de edición de usuario (sin editar el rol)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuario = ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // Acción Editar (POST): Actualiza los datos del usuario (excepto el rol)
        [HttpPost]
        public IActionResult Editar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                ActualizarUsuario(usuario);
                return RedirectToAction("Index");
            }
            return View(usuario);
        }

        // Acción AsignarRol: Muestra el formulario para asignar un nuevo rol
        [HttpGet]
        public IActionResult AsignarRol(int id)
        {
            var usuario = ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Prepara la lista de roles disponibles (Admin y Cliente)
            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new List<string> { "Admin", "Cliente" });

            return View(usuario);
        }

        // Acción AsignarRol (POST): Actualiza el rol del usuario
        [HttpPost]
        public IActionResult AsignarRol(int id, string nuevoRol)
        {
            var usuario = ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Actualiza el rol del usuario
            usuario.Rol = nuevoRol;
            ActualizarRolUsuario(usuario);

            return RedirectToAction("Index");
        }

        // Método privado para actualizar solo el rol del usuario en la base de datos
        private void ActualizarRolUsuario(Usuario usuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET Rol = @rol WHERE UsuarioID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", usuario.UsuarioID);
                    command.Parameters.AddWithValue("@rol", usuario.Rol);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método privado para obtener todos los usuarios
        private List<Usuario> ObtenerTodosLosUsuarios()
        {
            var usuarios = new List<Usuario>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT UsuarioID, Nombre, Apellidos, Email, Rol FROM Usuarios";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            UsuarioID = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellidos = reader.GetString(2),
                            Email = reader.GetString(3),
                            Rol = reader.GetString(4)
                        });
                    }
                }
            }

            return usuarios;
        }

        // Método privado para obtener un usuario por su ID
        private Usuario ObtenerUsuarioPorId(int id)
        {
            Usuario usuario = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT UsuarioID, Nombre, Apellidos, Email, Contraseña, Telefono, FechaNacimiento, Genero, Rol FROM Usuarios WHERE UsuarioID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

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

        // Acción Eliminar: Elimina un usuario por su ID
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var usuario = ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            EliminarUsuario(id);
            return RedirectToAction("Index");
        }

        // Método privado para eliminar un usuario de la base de datos
        private void EliminarUsuario(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Usuarios WHERE UsuarioID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }


        // Método privado para actualizar los datos de un usuario
        private void ActualizarUsuario(Usuario usuario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"UPDATE Usuarios 
                              SET Nombre = @nombre, Apellidos = @apellidos, Email = @correo, Contraseña = @contraseña, 
                                  Telefono = @telefono, FechaNacimiento = @fechaNacimiento, Genero = @genero 
                              WHERE UsuarioID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", usuario.UsuarioID);
                    command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@apellidos", usuario.Apellidos);
                    command.Parameters.AddWithValue("@correo", usuario.Email);
                    command.Parameters.AddWithValue("@contraseña", usuario.Contraseña);
                    command.Parameters.AddWithValue("@telefono", (object)usuario.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@fechaNacimiento", (object)usuario.FechaNacimiento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@genero", usuario.Genero);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
