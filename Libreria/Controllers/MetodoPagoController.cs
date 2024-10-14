using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Libreria.Models;
using Microsoft.Extensions.Configuration;

namespace Libreria.Controllers
{
    public class MetodoPagoController : Controller
    {
        private readonly string _connectionString;

        public MetodoPagoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar el formulario de agregar método de pago
        public IActionResult Crear()
        {
            return View();
        }

        // Acción para procesar el formulario de agregar método de pago
        [HttpPost]
        public async Task<IActionResult> Crear(MetodoPago metodoPago)
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                return View(metodoPago); // Regresar a la vista si hay errores en el modelo
            }

            try
            {
                // Parsear la fecha de vencimiento
                string[] fechaParts = metodoPago.FechaVencimiento.Split('/');
                if (fechaParts.Length != 2)
                {
                    ModelState.AddModelError(nameof(metodoPago.FechaVencimiento), "Formato de fecha inválido. Debe ser MM/AA.");
                    return View(metodoPago); // Regresa a la vista con el error
                }

                // Formatear la fecha a un tipo DateTime válido
                int mes = int.Parse(fechaParts[0]);
                int anio = int.Parse(fechaParts[1]) + 2000; // Asumiendo que el año ingresado es en el siglo 21
                DateTime fechaVencimiento = new DateTime(anio, mes, 1).AddMonths(1).AddDays(-1); // Último día del mes

                using (var conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO MetodosPago (TipoTarjeta, NumeroTarjeta, TitularTarjeta, FechaVencimiento, CVC) VALUES (@TipoTarjeta, @NumeroTarjeta, @TitularTarjeta, @FechaVencimiento, @CVC)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TipoTarjeta", metodoPago.TipoTarjeta);
                        cmd.Parameters.AddWithValue("@NumeroTarjeta", metodoPago.NumeroTarjeta);
                        cmd.Parameters.AddWithValue("@TitularTarjeta", metodoPago.TitularTarjeta);
                        cmd.Parameters.AddWithValue("@FechaVencimiento", fechaVencimiento);
                        cmd.Parameters.AddWithValue("@CVC", metodoPago.CVC);

                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Limpiar el estado del modelo para permitir un nuevo ingreso
                ModelState.Clear(); // Limpia el modelo
                return RedirectToAction("PagoExitoso"); // Redirigir a la vista de pago exitoso
            }
            catch (SqlException ex)
            {
                // Manejo de errores de SQL
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar el método de pago: " + ex.Message);
                return View(metodoPago); // Regresa a la vista con el error
            }
            catch (Exception ex)
            {
                // Manejo de otros errores
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(metodoPago); // Regresa a la vista con el error
            }
        }

        // Acción para mostrar la vista de pago exitoso
        public IActionResult PagoExitoso()
        {
            return View();
        }
    }
}
