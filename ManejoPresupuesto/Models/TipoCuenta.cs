using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta //: IValidatableObject
    {
        public int id { get; set; }
        //[StringLength( maximumLength:50, MinimumLength =3, ErrorMessage = "El Campo {0} debes estar entre {2} y {1} caracteres")]
        [Required(ErrorMessage = "El Campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")] // Atributo que valida el campo 'Nombre' en el lado del servidor, llamando al método 'VerificarExisteTipoCuenta' del controlador 'TiposCuentas'.
        public string Nombre { get; set; }
        public int UsuarioId { get; set;}
        public int Orden { get; set;}


       /* public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Nombre != null && Nombre.Length > 0)
            {
                var primeraLetra = Nombre[0].ToString();
                if (primeraLetra != primeraLetra.ToUpper())
                {
                    yield return new ValidationResult("La primera letra debe ser mayuscula", new[] { nameof(Nombre) });
                }
            }   
        }*/

        /* Pruebas de otras validaciones
        [Required(ErrorMessage = "El Campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El Campo {0} debe ser un correo valido")]
        public string Email { get; set; }
        [Url(ErrorMessage = "El Campo debe ser una URL valida")]
        public string Url { get; set; }
        [Range(minimum: 18, maximum: 120, ErrorMessage = "El Campo {0} debe estar entre {1} y {2} años")]
        public int Edad { get; set; }
        [CreditCard(ErrorMessage = "La tarjeta de credito no es valida")]
        [Display(Name = "Tarjeta de Credito")]
        public string TarjeCredito { get; set; }
        */
    }
}
