using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
    //la clase CuentaCreacionViewModel extiende la clase Cuenta y
    //añade una propiedad TiposCuentas que contiene una colección de elementos para ser utilizados en una lista desplegable,
    //para seleccionar el tipo de cuenta al crear una nueva cuenta.
}
