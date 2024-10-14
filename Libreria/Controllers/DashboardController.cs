using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
            int totalSeparadores = 0;
            int totalUsuarios = 0; // Agregada la variable totalUsuarios
            int totalPorEnviar = 0; // Total de pedidos por enviar
            int totalEnviados = 0; // Total de pedidos enviados
            int totalCompletados = 0; // Total de pedidos completados
            var librosPorGenero = new Dictionary<string, int>();

            // Aquí comienza el bloque using para abrir la conexión
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    // Obtener el total de libros
                    string queryTotalLibros = "SELECT SUM(Cantidad) FROM Libros";
                    using (SqlCommand cmdTotalLibros = new SqlCommand(queryTotalLibros, conn))
                    {
                        var resultTotalLibros = cmdTotalLibros.ExecuteScalar();
                        totalLibros = resultTotalLibros != DBNull.Value ? Convert.ToInt32(resultTotalLibros) : 0;
                    }

                    // Obtener el total de separadores
                    string queryTotalSeparador = "SELECT SUM(Cantidad) FROM Separador";
                    using (SqlCommand cmdTotalSeparador = new SqlCommand(queryTotalSeparador, conn))
                    {
                        var resultTotalSeparador = cmdTotalSeparador.ExecuteScalar();
                        totalSeparadores = resultTotalSeparador != DBNull.Value ? Convert.ToInt32(resultTotalSeparador) : 0;
                    }

                    // Obtener el total de usuarios
                    string queryTotalUsuarios = "SELECT COUNT(*) FROM Usuarios"; // Cambiar 'Usuarios' por el nombre real de la tabla si es diferente
                    using (SqlCommand cmdTotalUsuarios = new SqlCommand(queryTotalUsuarios, conn))
                    {
                        var resultTotalUsuarios = cmdTotalUsuarios.ExecuteScalar();
                        totalUsuarios = resultTotalUsuarios != DBNull.Value ? Convert.ToInt32(resultTotalUsuarios) : 0;
                    }

                    // Obtener el total de pedidos por enviar
                    string queryTotalPorEnviar = "SELECT COUNT(*) FROM Pedidos WHERE Estado = 'Por Enviar'"; // Ajusta la consulta según tu estructura de tabla
                    using (SqlCommand cmdTotalPorEnviar = new SqlCommand(queryTotalPorEnviar, conn))
                    {
                        var resultTotalPorEnviar = cmdTotalPorEnviar.ExecuteScalar();
                        totalPorEnviar = resultTotalPorEnviar != DBNull.Value ? Convert.ToInt32(resultTotalPorEnviar) : 0;
                    }

                    // Obtener el total de pedidos enviados
                    string queryTotalEnviados = "SELECT COUNT(*) FROM Pedidos WHERE Estado = 'Enviado'"; // Ajusta la consulta según tu estructura de tabla
                    using (SqlCommand cmdTotalEnviados = new SqlCommand(queryTotalEnviados, conn))
                    {
                        var resultTotalEnviados = cmdTotalEnviados.ExecuteScalar();
                        totalEnviados = resultTotalEnviados != DBNull.Value ? Convert.ToInt32(resultTotalEnviados) : 0;
                    }

                    // Obtener el total de pedidos completados
                    string queryTotalCompletados = "SELECT COUNT(*) FROM Pedidos WHERE Estado = 'Completado'"; // Ajusta la consulta según tu estructura de tabla
                    using (SqlCommand cmdTotalCompletados = new SqlCommand(queryTotalCompletados, conn))
                    {
                        var resultTotalCompletados = cmdTotalCompletados.ExecuteScalar();
                        totalCompletados = resultTotalCompletados != DBNull.Value ? Convert.ToInt32(resultTotalCompletados) : 0;
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
                catch (SqlException ex)
                {
                    // Log o manejo de cualquier otro error SQL
                    throw;
                }
            }

            // Pasar el total de libros, separadores, usuarios y pedidos a la vista
            ViewBag.TotalLibros = totalLibros;
            ViewBag.TotalSeparadores = totalSeparadores;
            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalPorEnviar = totalPorEnviar; // Total de pedidos por enviar
            ViewBag.TotalEnviados = totalEnviados; // Total de pedidos enviados
            ViewBag.TotalCompletados = totalCompletados; // Total de pedidos completados

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
