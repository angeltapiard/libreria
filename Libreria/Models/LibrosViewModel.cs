namespace Libreria.Models
{
    public class LibrosViewModel
    {
        public IEnumerable<Libro> Libros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
