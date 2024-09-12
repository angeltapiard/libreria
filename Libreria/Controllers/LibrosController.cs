using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libreria.Models;
using Microsoft.Extensions.Configuration;

namespace Libreria.Controllers
{
    public class LibrosController : Controller
    {
        private readonly string _connectionString;

        public LibrosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para mostrar la lista de libros
        public IActionResult Index()
        {
            List<Libro> libros = new List<Libro>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Titulo, Autor, Precio, Cantidad, Genero, Paginas, Encuadernacion, Portada, Sinopsis FROM Libros";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Libro libro = new Libro
                    {
                        Id = reader.GetInt32(0),
                        Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Autor = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Precio = reader.GetDecimal(3),
                        Cantidad = reader.GetInt32(4),
                        GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Paginas = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                        Encuadernacion = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Portada = reader.IsDBNull(8) ? null : (byte[])reader["Portada"],
                        Sinopsis = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };

                    libros.Add(libro);
                }
            }

            return View(libros);
        }

        // Acción para mostrar el formulario de agregar libro
        public IActionResult Crear()
        {
            return View();
        }

        // Acción para procesar el formulario de agregar libro
        [HttpPost]
        public IActionResult Crear(Libro libro, IFormFile portadaFile, string[] Generos)
        {
            if (portadaFile != null && portadaFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    portadaFile.CopyTo(ms);
                    libro.Portada = ms.ToArray();
                }
            }

            libro.Generos = Generos != null ? Generos.Select(g => Enum.Parse<GeneroLibro>(g)).ToList() : new List<GeneroLibro>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Libros (Titulo, Autor, Precio, Cantidad, Genero, Paginas, Encuadernacion, Portada, Sinopsis) VALUES (@Titulo, @Autor, @Precio, @Cantidad, @Genero, @Paginas, @Encuadernacion, @Portada, @Sinopsis)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Titulo", libro.Titulo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Autor", libro.Autor ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", libro.Precio);
                cmd.Parameters.AddWithValue("@Cantidad", libro.Cantidad);
                cmd.Parameters.AddWithValue("@Genero", libro.GenerosString ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Paginas", libro.Paginas.HasValue ? (object)libro.Paginas.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Encuadernacion", libro.Encuadernacion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Portada", libro.Portada ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Sinopsis", libro.Sinopsis ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // Acción para mostrar detalles de un libro
        public IActionResult Detalles(int id)
        {
            Libro libro = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Titulo, Autor, Precio, Cantidad, Genero, Paginas, Encuadernacion, Portada, Sinopsis FROM Libros WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    libro = new Libro
                    {
                        Id = reader.GetInt32(0),
                        Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Autor = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Precio = reader.GetDecimal(3),
                        Cantidad = reader.GetInt32(4),
                        GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Paginas = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                        Encuadernacion = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Portada = reader.IsDBNull(8) ? null : (byte[])reader["Portada"],
                        Sinopsis = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }
            }

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Acción para mostrar el formulario de edición de libro
        public IActionResult Editar(int id)
        {
            Libro libro = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Titulo, Autor, Precio, Cantidad, Genero, Paginas, Encuadernacion, Portada, Sinopsis FROM Libros WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    libro = new Libro
                    {
                        Id = reader.GetInt32(0),
                        Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Autor = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Precio = reader.GetDecimal(3),
                        Cantidad = reader.GetInt32(4),
                        GenerosString = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Paginas = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                        Encuadernacion = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Portada = reader.IsDBNull(8) ? null : (byte[])reader["Portada"],
                        Sinopsis = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }
            }

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        // Acción para procesar la edición de libro
        [HttpPost]
        public IActionResult Editar(Libro libro, IFormFile portadaFile, string[] Generos)
        {
            if (portadaFile != null && portadaFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    portadaFile.CopyTo(ms);
                    libro.Portada = ms.ToArray();
                }
            }
            else
            {
                // Si no se subió una nueva portada, mantener la portada actual
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Portada FROM Libros WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", libro.Id);

                    conn.Open();
                    var currentPortada = cmd.ExecuteScalar();
                    libro.Portada = currentPortada as byte[];
                }
            }

            libro.Generos = Generos != null ? Generos.Select(g => Enum.Parse<GeneroLibro>(g)).ToList() : new List<GeneroLibro>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Libros SET Titulo = @Titulo, Autor = @Autor, Precio = @Precio, Cantidad = @Cantidad, Genero = @Genero, Paginas = @Paginas, Encuadernacion = @Encuadernacion, Portada = @Portada, Sinopsis = @Sinopsis WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", libro.Id);
                cmd.Parameters.AddWithValue("@Titulo", libro.Titulo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Autor", libro.Autor ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", libro.Precio);
                cmd.Parameters.AddWithValue("@Cantidad", libro.Cantidad);
                cmd.Parameters.AddWithValue("@Genero", libro.GenerosString ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Paginas", libro.Paginas.HasValue ? (object)libro.Paginas.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Encuadernacion", libro.Encuadernacion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Portada", libro.Portada ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Sinopsis", libro.Sinopsis ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // Acción para eliminar un libro
        public IActionResult Eliminar(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Libros WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
