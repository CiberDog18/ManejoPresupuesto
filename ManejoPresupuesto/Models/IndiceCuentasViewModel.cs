namespace ManejoPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; } // Propiedad pública que almacena una colección de objetos Cuenta
        // Propiedades para calcular y almacenar los activos y pasivos por separado
        //public decimal Activos => Cuentas.Where(x => x.Balance > 0).Sum(x => x.Balance);
        //public decimal Pasivos => Cuentas.Where(x => x.Balance < 0).Sum(x => x.Balance);
     
        public decimal Balance => Cuentas.Sum(x => x.Balance); // Caulcula y devuelve la suma de los balances de todas las cuentas
    }
}
