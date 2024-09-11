using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Libreria.Models

{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Genero { get; set; }
        public int? Paginas { get; set; }
        public string Encuadernacion { get; set; }
        public byte[] Portada { get; set; }
        public string Sinopsis { get; set; }
    }
}
