using System;
using System.ComponentModel.DataAnnotations;

namespace Libreria.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; } // Primary key, auto-incremented

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } // Nombre del usuario

        [Required(ErrorMessage = "Los apellidos son requeridos.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
        public string Apellidos { get; set; } // Apellidos del usuario

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [StringLength(255, ErrorMessage = "El correo electrónico no puede exceder los 255 caracteres.")]
        public string Email { get; set; } // Correo electrónico del usuario

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(255, ErrorMessage = "La contraseña no puede exceder los 255 caracteres.")]
        public string Contraseña { get; set; } // Contraseña (almacenada hasheada)

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
        public string Telefono { get; set; } // Número de teléfono del usuario

        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; } // Fecha de nacimiento

        [Required(ErrorMessage = "El género es requerido.")]
        [RegularExpression("Masculino|Femenino", ErrorMessage = "El género debe ser 'Masculino' o 'Femenino'.")]
        public string Genero { get; set; } // Género del usuario

        [Required(ErrorMessage = "El rol es requerido.")]
        [RegularExpression("Admin|Cliente", ErrorMessage = "El rol debe ser 'Admin' o 'Cliente'.")]
        public string Rol { get; set; } // Rol del usuario (Admin o Cliente)

        public DateTime FechaRegistro { get; set; } = DateTime.Now; // Fecha de registro, por defecto es la fecha actual
    }
}
