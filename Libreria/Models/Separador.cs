using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models
{
    public class Separador
    {
        // Identificador único del separador
        [Key]
        public int SeparadorID { get; set; }

        // Nombre del separador
        [Required]
        public string Nombre { get; set; }

        // Precio del separador
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        // Cantidad disponible del separador
        [Required]
        public int Cantidad { get; set; }

        // Imagen del separador, almacenada como un byte array
        public byte[] Foto { get; set; }
    }
}
