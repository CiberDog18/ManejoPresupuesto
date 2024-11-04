using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    //Codigo utilizado en interfas para desplegar menús desplegables o listas de selección.
    public class TransaccionCreacionViewModel : Transaccion
    {
        // Lista de opciones seleccionables para las cuentas
        public IEnumerable<SelectListItem> Cuentas { get; set; }
        public IEnumerable<SelectListItem> Categorias { get; set; }
        

    }
}
