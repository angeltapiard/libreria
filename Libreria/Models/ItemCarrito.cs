using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models
{
    public class ItemCarrito
    {
        [Key]
        public int ItemCarritoID { get; set; } // Identificador único del ítem en el carrito

        [ForeignKey("Pedido")]
        public int PedidoID { get; set; } // Referencia al pedido (del cual forma parte este ítem)

        public int? LibroID { get; set; } // Referencia al libro (puede ser null si es un separador)
        public int? SeparadorID { get; set; } // Referencia al separador (puede ser null si es un libro)

        public DateTime FechaPedido { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; } // Cantidad de este ítem en el carrito

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; } // Total del ítem pedido (precio por cantidad)

        // Nuevas propiedades para los detalles del libro
        public string Titulo { get; set; } // Título del libro
        public string Autor { get; set; } // Autor del libro
        public decimal PrecioLibro { get; set; } // Precio del libro
        public byte[] PortadaLibro { get; set; } // Portada del libro en formato binario

        // Nuevas propiedades para los detalles del separador
        public string NombreSeparador { get; set; } // Nombre del separador
        public decimal PrecioSeparador { get; set; } // Precio del separador
        public byte[] FotoSeparador { get; set; } // Foto del separador en formato binario

        // Propiedades de navegación (opcional, en caso de que se use con Entity Framework)
        public virtual Pedido Pedido { get; set; } // Relación con la tabla Pedido
        public virtual Libro Libro { get; set; } // Relación con la tabla Libro
        public virtual Separador Separador { get; set; } // Relación con la tabla Separador

        public virtual ItemPedido ItemPedido { get; set; }
    }
}
