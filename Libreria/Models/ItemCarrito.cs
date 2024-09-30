using System.ComponentModel.DataAnnotations;

namespace Libreria.Models
{
    public class ItemCarrito
    {
        public int ItemCarritoID { get; set; } // Identificador único del ítem en el carrito
        public int CarritoID { get; set; } // Referencia al carrito
        public int? LibroID { get; set; } // Referencia al libro (puede ser null si es un separador)
        public int? SeparadorID { get; set; } // Referencia al separador (puede ser null si es un libro)

        [Required]
        public int Cantidad { get; set; } // Cantidad de este ítem en el carrito

        // Nuevas propiedades para los detalles del libro
        public string Titulo { get; set; } // Título del libro
        public string Autor { get; set; } // Autor del libro
        public decimal PrecioLibro { get; set; } // Precio del libro
        public byte[] PortadaLibro { get; set; } // Portada del libro en formato binario

        // Nuevas propiedades para los detalles del separador
        public string NombreSeparador { get; set; } // Nombre del separador
        public decimal PrecioSeparador { get; set; } // Precio del separador
        public byte[] FotoSeparador { get; set; } // Foto del separador en formato binario
    }
}
