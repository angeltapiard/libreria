using Libreria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace Libreria.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            // Obtener la cadena de conexión desde la configuración
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var libros = ObtenerUltimosLibros(20);

            return View(libros);
        }

        public IActionResult Detalles(int id)
        {
            var libro = ObtenerLibroPorId(id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Método para obtener los últimos N libros desde la base de datos
        private List<Libro> ObtenerUltimosLibros(int cantidad)
        {
            var libros = new List<Libro>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT TOP (@cantidad) Id, Titulo, Autor, Precio, Cantidad, Portada 
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
                                Portada = reader.IsDBNull(5) ? null : (byte[])reader[5]
                            });
                        }
                    }
                }
            }

            return libros;
        }

        // Método para obtener los detalles de un libro por su ID
        private Libro ObtenerLibroPorId(int id)
        {
            Libro libro = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT Id, Titulo, Autor, Precio, Cantidad, Portada, Paginas, Encuadernacion, Sinopsis
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
                                Portada = reader.IsDBNull(5) ? null : (byte[])reader[5],
                                Paginas = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Encuadernacion = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Sinopsis = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };
                        }
                    }
                }
            }

            return libro;
        }
    }
}
