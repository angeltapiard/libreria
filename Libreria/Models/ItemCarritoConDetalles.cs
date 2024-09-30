using System.ComponentModel.DataAnnotations;

namespace Libreria.Models
{
    public class ItemCarritoConDetalles
    {
        public int ItemCarritoID { get; set; }

        public int? LibroID { get; set; }
        public int? SeparadorID { get; set; }

        public int Cantidad { get; set; }

        // Detalles del libro
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public byte[] Portada { get; set; }

        // Detalles del separador
        public string Nombre { get; set; }
        public byte[] Foto { get; set; }
    }
}
