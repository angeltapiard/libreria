using System.Collections.Generic;

namespace Libreria.Models
{
    public class SearchViewModel
    {
        public List<Libro> Books { get; set; } = new List<Libro>();
        public List<Separador> Separators { get; set; } = new List<Separador>();
    }
}
