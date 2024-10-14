using System.ComponentModel.DataAnnotations;

namespace Libreria.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required]
        public string TipoTarjeta { get; set; }

        [Required]
        public string NumeroTarjeta { get; set; }

        [Required]
        public string TitularTarjeta { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Formato inválido. Debe ser MM/AA.")]
        public string FechaVencimiento { get; set; } // Mantener como string para formato MM/AA

        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVC debe tener 3 o 4 dígitos.")]
        public string CVC { get; set; }
    }
}
