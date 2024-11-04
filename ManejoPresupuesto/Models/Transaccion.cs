using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.DateTime)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("g"));
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "El campo Monto solo acepta números.")]
        public decimal Monto { get; set; }
        // Identificador de la categoría de la transacción, debe ser mayor o igual a 1
        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage ="Debe seleccionar una categoria")]
        public int CategoriaId { get; set; }
        [StringLength(maximumLength:1000, ErrorMessage ="La nota no puede pasar de {1} caracteres")]
        public string Nota { get; set; }
        [Display(Name = "Cuenta")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        public int CuentaId { get; set; }
        [Display(Name = "Tipo Operación")]
        public TipoOperacion tipoOperacionId { get; set; } = TipoOperacion.Ingreso;

        // Propiedades de navegación
        public string Cuenta { get; set; }
        public string Categoria { get; set; }
    }
}
