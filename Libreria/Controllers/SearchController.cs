using Libreria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Libreria.Controllers
{
    public class SearchController : Controller
    {
        private readonly string _connectionString;

        public SearchController(IConfiguration configuration)
        {
            // Obtener la cadena de conexión desde la configuración
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para obtener resultados de búsqueda en formato JSON
        public JsonResult GetResults(string query)
        {
            // Obtener los resultados de libros y separadores
            var libros = BuscarLibros(query);
            var separadores = BuscarSeparadores(query);

            // Combinar los resultados en una sola lista dinámica
            var resultados = new List<dynamic>();

            resultados.AddRange(libros);
            resultados.AddRange(separadores);

            // Devolver los resultados en formato JSON
            return Json(resultados);
        }

        // Método para buscar libros que coincidan con el término de búsqueda
        private List<dynamic> BuscarLibros(string query)
        {
            var libros = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string queryText = @"SELECT Id, Titulo, Autor, Precio, Portada 
                                     FROM Libros
                                     WHERE Titulo LIKE @query OR Autor LIKE @query";

                using (var command = new SqlCommand(queryText, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] portadaBytes = reader["Portada"] as byte[] ?? new byte[0];
                            string portadaBase64 = Convert.ToBase64String(portadaBytes);

                            libros.Add(new
                            {
                                titulo = reader.GetString(1),  // Título del libro
                                autor = reader.GetString(2),   // Autor del libro
                                precio = reader.GetDecimal(3), // Precio del libro
                                imagenBase64 = portadaBase64,  // Imagen en formato Base64
                                url = Url.Action("Detalles", "Home", new { id = reader.GetInt32(0) })  // URL del libro
                            });
                        }
                    }
                }
            }

            return libros;
        }

        // Método para buscar separadores que coincidan con el término de búsqueda
        private List<dynamic> BuscarSeparadores(string query)
        {
            var separadores = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string queryText = @"SELECT SeparadorID, Nombre, Precio, Foto 
                                     FROM Separador
                                     WHERE Nombre LIKE @query";

                using (var command = new SqlCommand(queryText, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] fotoBytes = reader["Foto"] as byte[] ?? new byte[0];
                            string fotoBase64 = Convert.ToBase64String(fotoBytes);

                            separadores.Add(new
                            {
                                nombre = reader.GetString(1),  // Nombre del separador
                                precio = reader.GetDecimal(2), // Precio del separador
                                imagenBase64 = fotoBase64,     // Imagen en formato Base64
                                url = Url.Action("DetallesSeparador", "Home", new { id = reader.GetInt32(0) })  // URL del separador
                            });
                        }
                    }
                }
            }

            return separadores;
        }
    }
}
