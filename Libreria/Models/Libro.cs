using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models
{
    // Enumeración de géneros literarios
    public enum GeneroLibro
    {
        Ficcion,
        Novela,
        Cuento,
        NovelaCorta,
        CienciaFiccion,
        Fantasia,
        Misterio,
        Suspenso,
        Terror,
        Romance,
        Historica,
        Biografia,
        Autobiografia,
        Memorias,
        Ensayo,
        Periodismo,
        Historia,
        Filosofia,
        Religion,
        Autoayuda,
        DivulgacionCientifica,
        InfantilJuvenil,
        CuentosDeHadas,
        Fabulas,
        Aventuras,
        RealismoMagico,
        NovelaGrafica,
        Poesia,
        Teatro,
        Cronica,
        Diario,
        Epistolar,
        Viajes,
        Cocina,
        Humor,
        Erotica
    }

    public class Libro
    {
        // Identificador del libro
        public int Id { get; set; }

        // Título del libro
        [Required]
        public string Titulo { get; set; }

        // Autor del libro
        [Required]
        public string Autor { get; set; }

        // Precio del libro
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        // Cantidad disponible del libro
        [Required]
        public int Cantidad { get; set; }

        // Lista de géneros seleccionados
        [Display(Name = "Géneros")]
        public List<GeneroLibro> Generos { get; set; } = new List<GeneroLibro>();

        // Número de páginas opcional
        public int? Paginas { get; set; }

        // Tipo de encuadernación
        public string Encuadernacion { get; set; }

        // Imagen de la portada del libro, almacenada como byte array
        public byte[] Portada { get; set; }

        // Sinopsis del libro
        public string Sinopsis { get; set; }

        // Propiedad calculada para convertir la lista de géneros en una cadena separada por comas
        [NotMapped]
        public string GenerosString
        {
            get { return string.Join(", ", Generos); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Crear una lista temporal para los géneros válidos
                    var generosValidos = new List<GeneroLibro>();

                    // Dividir la cadena por comas, limpiar espacios y procesar cada uno
                    foreach (var g in value.Split(','))
                    {
                        var trimmedGenre = g.Trim();

                        // Verificar si el género es válido en la enumeración
                        if (Enum.TryParse(trimmedGenre, true, out GeneroLibro genero))
                        {
                            generosValidos.Add(genero);
                        }
                        else
                        {
                            // Manejo de error para géneros no válidos (puedes personalizarlo)
                            Console.WriteLine($"Género no válido: {trimmedGenre}");
                        }
                    }

                    // Asignar los géneros válidos a la propiedad
                    Generos = generosValidos;
                }
                else
                {
                    // Si la cadena está vacía, limpiar la lista de géneros
                    Generos = new List<GeneroLibro>();
                }
            }
        }
    }
}
