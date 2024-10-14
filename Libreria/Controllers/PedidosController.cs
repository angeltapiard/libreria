using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Libreria.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Libreria.Controllers
{
    public class PedidosController : Controller
    {
        private readonly string _connectionString;

        public PedidosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar la lista de pedidos
        public async Task<IActionResult> Index(string estado = null)
        {
            var pedidos = new List<Pedido>();

            using (var conn = new SqlConnection(_connectionString))
            {
                // Consulta SQL con INNER JOIN para obtener los datos del pedido junto con el Email del usuario
                string query = @"
                SELECT p.PedidoID, p.UsuarioID, p.FechaPedido, p.Calle, p.Municipio, p.Provincia, p.Estado, p.Total, u.Email
                FROM Pedidos p
                INNER JOIN Usuarios u ON p.UsuarioID = u.UsuarioID";

                // Agregar condición de filtrado si se proporciona un estado
                if (!string.IsNullOrEmpty(estado))
                {
                    query += " WHERE p.Estado = @Estado"; // Agrega la condición de estado
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(estado))
                    {
                        cmd.Parameters.AddWithValue("@Estado", estado); // Establecer el parámetro del estado
                    }

                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var pedido = new Pedido
                            {
                                PedidoID = reader.GetInt32(0),
                                UsuarioID = reader.GetInt32(1),
                                FechaPedido = reader.GetDateTime(2),
                                Calle = reader.GetString(3),
                                Municipio = reader.GetString(4),
                                Provincia = reader.GetString(5),
                                Estado = reader.GetString(6),
                                Total = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                                Email = reader.GetString(8)
                            };
                            pedidos.Add(pedido);
                        }
                    }
                }
            }

            return View(pedidos); // Pasar la lista de pedidos a la vista
        }

        // Acción para mostrar el formulario de agregar pedido
        public IActionResult Crear()
        {
            var modelo = new Pedido
            {
                Email = User.Identity.Name,
                Telefono = GetUserPhoneNumber()
            };

            return View(modelo);
        }

        // Acción para procesar el formulario de agregar pedido
        [HttpPost]
        public async Task<IActionResult> Crear(Pedido pedido)
        {
            string userEmail = User.Identity.Name;

            if (ModelState.IsValid && !string.IsNullOrEmpty(userEmail))
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                    INSERT INTO Pedidos (FechaPedido, Calle, Municipio, Provincia, Estado, UsuarioID) 
                    VALUES (@FechaPedido, @Calle, @Municipio, @Provincia, @Estado, 
                            (SELECT UsuarioID FROM Usuarios WHERE Email = @Email))";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FechaPedido", pedido.FechaPedido);
                        cmd.Parameters.AddWithValue("@Calle", pedido.Calle ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Municipio", pedido.Municipio ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Provincia", pedido.Provincia ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Estado", pedido.Estado ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", userEmail);

                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("Crear", "MetodoPago");

            }

            return View(pedido);
        }

        // Acción para editar el estado del pedido
        [HttpPost]
        public async Task<IActionResult> EditarEstado(int pedidoId, string nuevoEstado)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Pedidos SET Estado = @Estado WHERE PedidoID = @PedidoID";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@PedidoID", pedidoId);

                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Redirigir a la vista de índice después de editar
            return RedirectToAction(nameof(Index));
        }

        // Método privado para obtener el número de teléfono del usuario actual
        private string GetUserPhoneNumber()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Telefono FROM Usuarios WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", User.Identity.Name);
                    var result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        // Acción para mostrar la lista de pedidos del cliente
        public async Task<IActionResult> PedidosCliente(string estado = null)
        {
            var userEmail = User.Identity.Name; // Obtener el email del usuario autenticado
            var pedidos = new List<Pedido>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT p.PedidoID, p.FechaPedido, p.Calle, p.Municipio, p.Provincia, p.Estado, p.Total
                FROM Pedidos p
                INNER JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
                WHERE u.Email = @Email";  // Hacer el join con la tabla Usuarios

                // Agregar condición de filtrado si se proporciona un estado
                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND p.Estado = @Estado"; // Agregar condición de estado
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", userEmail);

                    if (!string.IsNullOrEmpty(estado))
                    {
                        cmd.Parameters.AddWithValue("@Estado", estado); // Establecer el parámetro del estado
                    }

                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var pedido = new Pedido
                            {
                                PedidoID = reader.GetInt32(0),
                                FechaPedido = reader.GetDateTime(1),
                                Calle = reader.GetString(2),
                                Municipio = reader.GetString(3),
                                Provincia = reader.GetString(4),
                                Estado = reader.GetString(5),
                                Total = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
                            };
                            pedidos.Add(pedido);
                        }
                    }
                }
            }

            return View(pedidos); // Devolver la lista de pedidos a la vista
        }
    }
}
