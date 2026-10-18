using System.ComponentModel.DataAnnotations;

namespace SIGEVA_API.Models
{
    public class VerificarCodigoModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "El código debe contener exactamente 6 dígitos.")]
        public string Codigo { get; set; } = string.Empty;
    }
}