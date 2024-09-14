using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using Libreria.Models;
using Microsoft.Extensions.Configuration;

namespace Libreria.Controllers
{
    public class SeparadoresController : Controller
    {
        private readonly string _connectionString;

        public SeparadoresController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar la lista de separadores
        public IActionResult Index()
        {
            List<Separador> separadores = new List<Separador>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT SeparadorID, Nombre, Precio, Cantidad, Foto FROM Separador";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Separador separador = new Separador
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

            return View(separadores);
        }

        // Acción para mostrar el formulario de agregar separador
        public IActionResult Crear()
        {
            return View();
        }

        // Acción para procesar el formulario de agregar separador
        [HttpPost]
        public IActionResult Crear(Separador separador, IFormFile fotoFile)
        {
            if (fotoFile != null && fotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    fotoFile.CopyTo(ms);
                    separador.Foto = ms.ToArray();
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Separador (Nombre, Precio, Cantidad, Foto) VALUES (@Nombre, @Precio, @Cantidad, @Foto)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", separador.Nombre ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", separador.Precio);
                cmd.Parameters.AddWithValue("@Cantidad", separador.Cantidad);
                cmd.Parameters.AddWithValue("@Foto", separador.Foto ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // Acción para mostrar el formulario de edición de separador
        public IActionResult Editar(int id)
        {
            Separador separador = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT SeparadorID, Nombre, Precio, Cantidad, Foto FROM Separador WHERE SeparadorID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
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

            if (separador == null)
            {
                return NotFound();
            }

            return View(separador);
        }

        // Acción para procesar la edición de separador
        [HttpPost]
        public IActionResult Editar(Separador separador, IFormFile fotoFile)
        {
            if (fotoFile != null && fotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    fotoFile.CopyTo(ms);
                    separador.Foto = ms.ToArray();
                }
            }
            else
            {
                // Si no se subió una nueva foto, mantener la foto actual
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Foto FROM Separador WHERE SeparadorID = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", separador.SeparadorID);

                    conn.Open();
                    var currentFoto = cmd.ExecuteScalar();
                    separador.Foto = currentFoto as byte[];
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Separador SET Nombre = @Nombre, Precio = @Precio, Cantidad = @Cantidad, Foto = @Foto WHERE SeparadorID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", separador.SeparadorID);
                cmd.Parameters.AddWithValue("@Nombre", separador.Nombre ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", separador.Precio);
                cmd.Parameters.AddWithValue("@Cantidad", separador.Cantidad);
                cmd.Parameters.AddWithValue("@Foto", separador.Foto ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // Acción para eliminar un separador
        public IActionResult Eliminar(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Separador WHERE SeparadorID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
