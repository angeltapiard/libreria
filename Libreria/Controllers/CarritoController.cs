using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Libreria.Models;
using System;
using System.Collections.Generic;

namespace Libreria.Controllers
{
    [Authorize] // Solo usuarios autenticados pueden acceder
    public class CarritoController : Controller
    {
        private readonly string _connectionString;

        public CarritoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para ver el carrito
        public IActionResult Index()
        {
            var carritoItems = new List<ItemCarrito>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    int usuarioId = GetCurrentUserId(); // Método para obtener el ID del usuario actual

                    // Obtener el carrito del usuario actual
                    string queryCarrito = "SELECT CarritoID FROM Carrito WHERE UsuarioID = @UsuarioID";
                    using (SqlCommand cmd = new SqlCommand(queryCarrito, conn))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                        object result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            // Si no existe un carrito, podemos crear uno nuevo
                            string crearCarrito = "INSERT INTO Carrito (UsuarioID) OUTPUT INSERTED.CarritoID VALUES (@UsuarioID)";
                            using (SqlCommand cmdCrear = new SqlCommand(crearCarrito, conn))
                            {
                                cmdCrear.Parameters.AddWithValue("@UsuarioID", usuarioId);
                                result = cmdCrear.ExecuteScalar(); // Capturamos el nuevo CarritoID
                            }
                        }

                        int carritoId = (int)result;

                        // Obtener los items del carrito con detalles de libros y separadores
                        string queryItems = "SELECT ic.ItemCarritoID, ic.LibroID, ic.SeparadorID, SUM(ic.Cantidad) AS Cantidad, " +
                                             "l.titulo, l.autor, l.precio AS PrecioLibro, l.portada AS PortadaLibro, " +
                                             "s.Nombre AS NombreSeparador, s.Precio AS PrecioSeparador, s.Foto AS FotoSeparador " +
                                             "FROM ItemsCarrito ic " +
                                             "LEFT JOIN Libros l ON ic.LibroID = l.id " +
                                             "LEFT JOIN Separador s ON ic.SeparadorID = s.SeparadorID " +
                                             "WHERE ic.CarritoID = @CarritoID " +
                                             "GROUP BY ic.ItemCarritoID, ic.LibroID, ic.SeparadorID, l.titulo, l.autor, l.precio, l.portada, s.Nombre, s.Precio, s.Foto";

                        using (SqlCommand cmdItems = new SqlCommand(queryItems, conn))
                        {
                            cmdItems.Parameters.AddWithValue("@CarritoID", carritoId);
                            using (var reader = cmdItems.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var item = new ItemCarrito
                                    {
                                        ItemCarritoID = reader.GetInt32(0),
                                        LibroID = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                        SeparadorID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                        Cantidad = reader.GetInt32(3),
                                        Titulo = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        Autor = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        PrecioLibro = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                                        PortadaLibro = reader.IsDBNull(7) ? null : (byte[])reader[7],
                                        NombreSeparador = reader.IsDBNull(8) ? null : reader.GetString(8),
                                        PrecioSeparador = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                                        FotoSeparador = reader.IsDBNull(10) ? null : (byte[])reader[10]
                                    };
                                    carritoItems.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores SQL
                    ModelState.AddModelError("", "Error al cargar el carrito: " + ex.Message);
                    return View(carritoItems); // Retornar la vista incluso en caso de error
                }
            }

            return View(carritoItems); // Pasar los ítems del carrito a la vista
        }

        // Método para agregar un item al carrito
        [HttpPost]
        public IActionResult AgregarItem(int? libroId, int? separadorId, int cantidad)
        {
            int usuarioId = GetCurrentUserId(); // Método para obtener el ID del usuario actual

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    // Obtener el carrito del usuario actual o crear uno nuevo
                    string queryCarrito = "IF NOT EXISTS (SELECT * FROM Carrito WHERE UsuarioID = @UsuarioID) " +
                                          "BEGIN " +
                                          "INSERT INTO Carrito (UsuarioID) VALUES (@UsuarioID); " +
                                          "END; " +
                                          "SELECT CarritoID FROM Carrito WHERE UsuarioID = @UsuarioID";

                    int carritoId;
                    using (SqlCommand cmd = new SqlCommand(queryCarrito, conn))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                        carritoId = (int)cmd.ExecuteScalar();
                    }

                    // Verificar si el item ya existe en el carrito
                    string queryExistencia = "SELECT ItemCarritoID, Cantidad FROM ItemsCarrito " +
                                              "WHERE CarritoID = @CarritoID " +
                                              "AND (LibroID = @LibroID OR SeparadorID = @SeparadorID)";

                    int? itemCarritoId = null;
                    int cantidadExistente = 0;

                    using (SqlCommand cmdExistencia = new SqlCommand(queryExistencia, conn))
                    {
                        cmdExistencia.Parameters.AddWithValue("@CarritoID", carritoId);
                        cmdExistencia.Parameters.AddWithValue("@LibroID", (object)libroId ?? DBNull.Value);
                        cmdExistencia.Parameters.AddWithValue("@SeparadorID", (object)separadorId ?? DBNull.Value);

                        using (var reader = cmdExistencia.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                itemCarritoId = reader.GetInt32(0); // ID del item existente
                                cantidadExistente = reader.GetInt32(1); // Cantidad existente
                            }
                        }
                    }

                    if (itemCarritoId.HasValue)
                    {
                        // Si el item ya existe, actualizar la cantidad
                        string queryActualizarCantidad = "UPDATE ItemsCarrito SET Cantidad = @Cantidad WHERE ItemCarritoID = @ItemCarritoID";
                        using (SqlCommand cmdActualizar = new SqlCommand(queryActualizarCantidad, conn))
                        {
                            cmdActualizar.Parameters.AddWithValue("@Cantidad", cantidadExistente + cantidad);
                            cmdActualizar.Parameters.AddWithValue("@ItemCarritoID", itemCarritoId.Value);
                            cmdActualizar.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Si el item no existe, agregar uno nuevo
                        string queryAgregarItem = "INSERT INTO ItemsCarrito (CarritoID, LibroID, SeparadorID, Cantidad) " +
                                                   "VALUES (@CarritoID, @LibroID, @SeparadorID, @Cantidad)";
                        using (SqlCommand cmdAgregarItem = new SqlCommand(queryAgregarItem, conn))
                        {
                            cmdAgregarItem.Parameters.AddWithValue("@CarritoID", carritoId);
                            cmdAgregarItem.Parameters.AddWithValue("@LibroID", (object)libroId ?? DBNull.Value);
                            cmdAgregarItem.Parameters.AddWithValue("@SeparadorID", (object)separadorId ?? DBNull.Value);
                            cmdAgregarItem.Parameters.AddWithValue("@Cantidad", cantidad);
                            cmdAgregarItem.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores SQL
                    ModelState.AddModelError("", "Error al agregar el item al carrito: " + ex.Message);
                    return RedirectToAction("Index"); // Redirigir a la vista del carrito, aunque haya un error
                }
            }

            return RedirectToAction("Index"); // Redirigir a la vista del carrito
        }

        // Método para agregar un libro al carrito
        [HttpPost]
        public IActionResult AgregarLibro(int libroId)
        {
            if (libroId <= 0)
            {
                ModelState.AddModelError("", "El ID del libro no es válido.");
                return RedirectToAction("Index");
            }

            // Lógica para agregar el libro al carrito
            return AgregarItem(libroId, null, 1); // Asumiendo que la cantidad es 1
        }

        // Método para agregar un separador al carrito
        [HttpPost]
        public IActionResult AgregarSeparador(int separadorId)
        {
            if (separadorId <= 0)
            {
                ModelState.AddModelError("", "El ID del separador no es válido.");
                return RedirectToAction("Index");
            }

            // Lógica para agregar el separador al carrito
            return AgregarItem(null, separadorId, 1); // Asumiendo que la cantidad es 1
        }

        // Método para eliminar un item del carrito
        [HttpPost]
        public IActionResult EliminarItem(int itemId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                try
                {
                    string queryEliminarItem = "DELETE FROM ItemsCarrito WHERE ItemCarritoID = @ItemCarritoID";
                    using (SqlCommand cmd = new SqlCommand(queryEliminarItem, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemCarritoID", itemId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // Manejo de errores SQL
                    ModelState.AddModelError("", "Error al eliminar el item del carrito: " + ex.Message);
                    return RedirectToAction("Index"); // Redirigir a la vista del carrito, aunque haya un error
                }
            }

            return RedirectToAction("Index"); // Redirigir a la vista del carrito
        }

        // Método privado para obtener el ID del usuario actual
        private int GetCurrentUserId()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Cambié la consulta para que use UsuarioID en lugar de Id
                string query = "SELECT UsuarioID FROM Usuarios WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", User.Identity.Name);
                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            throw new Exception("Usuario no encontrado.");
        }
    }
}
