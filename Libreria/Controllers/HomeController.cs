using Libreria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Libreria.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar la página principal con los últimos libros y separadores
        public IActionResult Index()
        {
            var libros = ObtenerUltimosLibros(20);
            var separadores = ObtenerUltimosSeparadores(10);

            ViewData["Separadores"] = separadores;
            return View(libros);
        }

        // Acción para mostrar detalles de un libro específico
        public IActionResult Detalles(int id)
        {
            var libro = ObtenerLibroPorId(id);

            if (libro == null)
            {
                return NotFound();
            }

            var librosRelacionados = ObtenerLibrosPorGeneros(libro.Generos, 9, id);
            ViewData["LibrosRelacionados"] = librosRelacionados;

            return View(libro);
        }

        // Acción para mostrar detalles de un separador específico
        public IActionResult DetallesSeparador(int id)
        {
            var separador = ObtenerSeparadorPorId(id);

            if (separador == null)
            {
                return NotFound();
            }

            var ultimosSeparadores = ObtenerUltimosSeparadores(6);
            ViewData["Separadores"] = ultimosSeparadores;

            return View(separador);
        }

        // Acción para ver todos los libros
        public IActionResult VerTodosLosLibros()
        {
            var libros = ObtenerUltimosLibros(100);
            return View(libros);
        }

        // Acción para ver todos los separadores
        public IActionResult VerTodosLosSeparadores()
        {
            var separadores = ObtenerUltimosSeparadores(100);
            return View(separadores);
        }

        // Acción para realizar búsqueda dinámica de libros (respuesta en JSON)
        [HttpGet]
        public async Task<IActionResult> GetResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return Json(new List<object>()); // Retornar vacío si la consulta es inválida
            }

            var libros = await BuscarLibros(query); // Método para búsqueda
            var results = libros.Select(b => new
            {
                titulo = b.Titulo,
                autor = b.Autor,
                precio = b.Precio.ToString("C"),
                url = Url.Action("Detalles", new { id = b.Id }),
                imagenBase64 = Convert.ToBase64String(b.Portada) // Imagen en base64
            }).ToList();

            return Json(results);
        }

        // Métodos privados de consulta a la base de datos
        private List<Libro> ObtenerUltimosLibros(int cantidad)
        {
            var libros = new List<Libro>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT TOP (@cantidad) Id, Titulo, Autor, Precio, Cantidad, Genero, Portada 
                                 FROM Libros
                                 ORDER BY Id DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cantidad", cantidad);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            libros.Add(new Libro
                            {
                                Id = reader.GetInt32(0),
                                Titulo = reader.GetString(1),
                                Autor = reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                Cantidad = reader.GetInt32(4),
                                GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Portada = reader.IsDBNull(6) ? null : (byte[])reader[6]
                            });
                        }
                    }
                }
            }

            return libros;
        }

        private Libro ObtenerLibroPorId(int id)
        {
            Libro libro = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT Id, Titulo, Autor, Precio, Cantidad, Genero, Paginas, Encuadernacion, Portada, Sinopsis
                                 FROM Libros
                                 WHERE Id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            libro = new Libro
                            {
                                Id = reader.GetInt32(0),
                                Titulo = reader.GetString(1),
                                Autor = reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                Cantidad = reader.GetInt32(4),
                                GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Paginas = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Encuadernacion = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Portada = reader.IsDBNull(8) ? null : (byte[])reader[8],
                                Sinopsis = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            if (!string.IsNullOrEmpty(libro.GenerosString))
                            {
                                libro.Generos = libro.GenerosString.Split(',')
                                    .Select(g => (GeneroLibro)Enum.Parse(typeof(GeneroLibro), g.Trim(), true))
                                    .ToList();
                            }
                        }
                    }
                }
            }

            return libro;
        }

        private List<Libro> ObtenerLibrosPorGeneros(List<GeneroLibro> generos, int cantidad, int libroId)
        {
            var librosRelacionados = new List<Libro>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var generosList = string.Join(",", generos.Select(g => $"'{g}'"));

                string query = @"
                    SELECT TOP (@cantidad) Id, Titulo, Autor, Precio, Cantidad, Genero, Portada 
                    FROM Libros
                    WHERE Id != @libroId
                    AND Genero IN (" + generosList + @")
                    ORDER BY Id DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cantidad", cantidad);
                    command.Parameters.AddWithValue("@libroId", libroId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            librosRelacionados.Add(new Libro
                            {
                                Id = reader.GetInt32(0),
                                Titulo = reader.GetString(1),
                                Autor = reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                Cantidad = reader.GetInt32(4),
                                GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Portada = reader.IsDBNull(6) ? null : (byte[])reader[6]
                            });
                        }
                    }
                }
            }

            return librosRelacionados;
        }

        private List<Separador> ObtenerUltimosSeparadores(int cantidad)
        {
            var separadores = new List<Separador>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT TOP (@cantidad) SeparadorID, Nombre, Precio, Cantidad, Foto 
                                 FROM Separador
                                 ORDER BY SeparadorID DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cantidad", cantidad);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            separadores.Add(new Separador
                            {
                                SeparadorID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Cantidad = reader.GetInt32(3),
                                Foto = reader.IsDBNull(4) ? null : (byte[])reader[4]
                            });
                        }
                    }
                }
            }

            return separadores;
        }

        private Separador ObtenerSeparadorPorId(int id)
        {
            Separador separador = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT SeparadorID, Nombre, Precio, Cantidad, Foto 
                                 FROM Separador
                                 WHERE SeparadorID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            separador = new Separador
                            {
                                SeparadorID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Cantidad = reader.GetInt32(3),
                                Foto = reader.IsDBNull(4) ? null : (byte[])reader[4]
                            };
                        }
                    }
                }
            }

            return separador;
        }

        // Método de búsqueda dinámica de libros
        private async Task<List<Libro>> BuscarLibros(string query)
        {
            var libros = new List<Libro>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string searchQuery = @"
                    SELECT Id, Titulo, Autor, Precio, Cantidad, Portada 
                    FROM Libros
                    WHERE Titulo LIKE @query OR Autor LIKE @query";

                using (var command = new SqlCommand(searchQuery, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            libros.Add(new Libro
                            {
                                Id = reader.GetInt32(0),
                                Titulo = reader.GetString(1),
                                Autor = reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                Cantidad = reader.GetInt32(4),
                                Portada = reader.IsDBNull(5) ? null : (byte[])reader[5]
                            });
                        }
                    }
                }
            }

            return libros;
        }
    }
}
