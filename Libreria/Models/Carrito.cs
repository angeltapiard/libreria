using System;

namespace Libreria.Models
{
    public class Carrito
    {
        public int CarritoID { get; set; } // Identificador único del carrito
        public int UsuarioID { get; set; } // Referencia al usuario que posee el carrito
        public DateTime FechaCreacion { get; set; } = DateTime.Now; // Fecha de creación del carrito
    }
}
