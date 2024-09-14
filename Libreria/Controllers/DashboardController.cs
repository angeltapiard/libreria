using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Libreria.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string _connectionString;

        public DashboardController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar el dashboard
        public IActionResult Index()
        {
            int totalLibros = 0;
            var librosPorGenero = new Dictionary<string, int>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Obtener el total de libros
                string queryTotal = "SELECT SUM(Cantidad) FROM Libros";
                using (SqlCommand cmdTotal = new SqlCommand(queryTotal, conn))
                {
                    var resultTotal = cmdTotal.ExecuteScalar();
                    totalLibros = resultTotal != DBNull.Value ? Convert.ToInt32(resultTotal) : 0;
                }

                // Obtener la cantidad de libros por género
                string queryGenero = @"
                    SELECT Genero, COUNT(*) AS Cantidad
                    FROM (
                        SELECT TRIM(value) AS Genero
                        FROM Libros
                        CROSS APPLY STRING_SPLIT(Genero, ',')
                    ) AS Gen
                    GROUP BY Genero";

                using (SqlCommand cmdGenero = new SqlCommand(queryGenero, conn))
                {
                    using (var reader = cmdGenero.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string genero = reader.GetString(0).Trim();
                            int cantidad = reader.GetInt32(1);

                            if (librosPorGenero.ContainsKey(genero))
                            {
                                librosPorGenero[genero] += cantidad;
                            }
                            else
                            {
                                librosPorGenero[genero] = cantidad;
                            }
                        }
                    }
                }
            }

            // Pasar el total de libros y libros por género a la vista
            ViewBag.TotalLibros = totalLibros;

            // Preparar los datos para el gráfico
            ViewBag.LibrosPorGenero = new
            {
                Genres = librosPorGenero.Keys.ToArray(),
                Quantities = librosPorGenero.Values.ToArray()
            };

            return View();
        }
    }
}
