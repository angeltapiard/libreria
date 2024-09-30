using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using Libreria.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Libreria.Controllers
{
    public class SeparadoresController : Controller
    {
        private readonly string _connectionString;

        public SeparadoresController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar la lista de separadores con paginación
        public async Task<IActionResult> Index(int pagina = 1)
        {
            int cantidadPorPagina = 9;
            List<Separador> separadores = new List<Separador>();

            using (var conn = new SqlConnection(_connectionString))
            {
                // Calcular el índice de inicio para la página actual
                int skip = (pagina - 1) * cantidadPorPagina;

                // Consulta para obtener los separadores con paginación
                string query = "SELECT SeparadorID, Nombre, Precio, Cantidad, Foto " +
                               "FROM Separador " +
                               "ORDER BY Nombre " +
                               "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Skip", skip);
                    cmd.Parameters.AddWithValue("@Take", cantidadPorPagina);
                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var separador = new Separador
                            {
                                SeparadorID = reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Cantidad = reader.GetInt32(3),
                                Foto = reader.IsDBNull(4) ? null : (byte[])reader["Foto"]
                            };

                            separadores.Add(separador);
                        }
                    }
                }
            }

            // Obtener el total de separadores para calcular el número total de páginas
            int totalSeparadores;
            using (var conn = new SqlConnection(_connectionString))
            {
                string countQuery = "SELECT COUNT(*) FROM Separador";
                using (var countCmd = new SqlCommand(countQuery, conn))
                {
                    conn.Open();
                    totalSeparadores = (int)await countCmd.ExecuteScalarAsync();
                }
            }

            int totalPaginas = (int)Math.Ceiling((double)totalSeparadores / cantidadPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View("Index", separadores); // Devuelve la vista Index.cshtml
        }

        // Acción para mostrar la vista de separadores (página secundaria)
        public async Task<IActionResult> Separadores(int pagina = 1)
        {
            int cantidadPorPagina = 9;
            List<Separador> separadores = new List<Separador>();

            using (var conn = new SqlConnection(_connectionString))
            {
                // Calcular el índice de inicio para la página actual
                int skip = (pagina - 1) * cantidadPorPagina;

                // Consulta para obtener los separadores con paginación
                string query = "SELECT SeparadorID, Nombre, Precio, Cantidad, Foto " +
                               "FROM Separador " +
                               "ORDER BY Nombre " +
                               "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Skip", skip);
                    cmd.Parameters.AddWithValue("@Take", cantidadPorPagina);
                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var separador = new Separador
                            {
                                SeparadorID = reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Cantidad = reader.GetInt32(3),
                                Foto = reader.IsDBNull(4) ? null : (byte[])reader["Foto"]
                            };

                            separadores.Add(separador);
                        }
                    }
                }
            }

            // Obtener el total de separadores para calcular el número total de páginas
            int totalSeparadores;
            using (var conn = new SqlConnection(_connectionString))
            {
                string countQuery = "SELECT COUNT(*) FROM Separador";
                using (var countCmd = new SqlCommand(countQuery, conn))
                {
                    conn.Open();
                    totalSeparadores = (int)await countCmd.ExecuteScalarAsync();
                }
            }

            int totalPaginas = (int)Math.Ceiling((double)totalSeparadores / cantidadPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View("Separador", separadores); // Devuelve la vista Separador.cshtml
        }

        // Acción para mostrar el formulario de agregar separador
        public IActionResult Crear()
        {
            return View();
        }

        // Acción para procesar el formulario de agregar separador
        [HttpPost]
        public async Task<IActionResult> Crear(Separador separador, IFormFile fotoFile)
        {
            if (fotoFile != null && fotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await fotoFile.CopyToAsync(ms);
                    separador.Foto = ms.ToArray();
                }
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Separador (Nombre, Precio, Cantidad, Foto) VALUES (@Nombre, @Precio, @Cantidad, @Foto)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", separador.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", separador.Precio);
                    cmd.Parameters.AddWithValue("@Cantidad", separador.Cantidad);
                    cmd.Parameters.AddWithValue("@Foto", separador.Foto ?? (object)DBNull.Value);

                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index"); // Redirige a la vista Index
        }

        // Acción para mostrar el formulario de edición de separador
        public async Task<IActionResult> Editar(int id)
        {
            Separador separador = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT SeparadorID, Nombre, Precio, Cantidad, Foto FROM Separador WHERE SeparadorID = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            separador = new Separador
                            {
                                SeparadorID = reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Cantidad = reader.GetInt32(3),
                                Foto = reader.IsDBNull(4) ? null : (byte[])reader["Foto"]
                            };
                        }
                    }
                }
            }

            if (separador == null)
            {
                return NotFound();
            }

            return View(separador); // Devuelve la vista Editar.cshtml
        }

        // Acción para procesar la edición de separador
        [HttpPost]
        public async Task<IActionResult> Editar(Separador separador, IFormFile fotoFile)
        {
            if (fotoFile != null && fotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await fotoFile.CopyToAsync(ms);
                    separador.Foto = ms.ToArray();
                }
            }
            else
            {
                // Si no se subió una nueva foto, mantener la foto actual
                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Foto FROM Separador WHERE SeparadorID = @Id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", separador.SeparadorID);

                        conn.Open();
                        var currentFoto = await cmd.ExecuteScalarAsync();
                        separador.Foto = currentFoto as byte[];
                    }
                }
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Separador SET Nombre = @Nombre, Precio = @Precio, Cantidad = @Cantidad, Foto = @Foto WHERE SeparadorID = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", separador.SeparadorID);
                    cmd.Parameters.AddWithValue("@Nombre", separador.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", separador.Precio);
                    cmd.Parameters.AddWithValue("@Cantidad", separador.Cantidad);
                    cmd.Parameters.AddWithValue("@Foto", separador.Foto ?? (object)DBNull.Value);

                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index"); // Redirige a la vista Index
        }

        // Acción para eliminar un separador
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Separador WHERE SeparadorID = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index"); // Redirige a la vista Index
        }
    }
}
