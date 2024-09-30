namespace Libreria.Models
{
    public class SeparadoresViewModel
    {
        public IEnumerable<Separador> Separadores { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        // Calcula el total de elementos, puedes agregar este valor desde el controlador
        public int TotalElementos { get; set; }

        // Calcula el número de elementos por página
        public int ElementosPorPagina { get; set; } = 10;

        // Calcula el número total de páginas
        public void CalcularTotalPaginas()
        {
            if (TotalElementos > 0 && ElementosPorPagina > 0)
            {
                TotalPaginas = (int)Math.Ceiling((double)TotalElementos / ElementosPorPagina);
            }
            else
            {
                TotalPaginas = 1;
            }
        }
    }
}
