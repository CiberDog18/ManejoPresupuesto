namespace ManejoPresupuesto.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set;}
        // Esto almacena las transacciones agrupadas por fecha.
        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }
        // Calculamos el balance de depósitos sumando los BalanceDepositos de todas las transacciones agrupadas.
        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(x => x.BalanceDepositos);
        // Calculamos el balance de retiros sumando los BalanceRetiros de todas las transacciones agrupadas.
        public decimal BalanceRetiros => TransaccionesAgrupadas.Sum(x => x.BalanceRetiros);
        // Calculamos el balance total restando el balance de retiros del balance de depósitos.
        public decimal Total => BalanceDepositos - BalanceRetiros;

        // Definimos una clase interna TransaccionesPorFecha que agrupa transacciones por fecha.
        public class TransaccionesPorFecha {
            // almacena la fecha de la transacción.
            public DateTime FechaTransaccion {  get; set; }
            // Propiedad Transacciones que es un conjunto de Transaccion. Esto almacena las transacciones.
            public IEnumerable<Transaccion> Transacciones { get; set;}
            // Calculamos el balance de depósitos para esta fecha filtrando y sumando las transacciones de tipo ingreso.
            public decimal BalanceDepositos => Transacciones.Where(x => x.tipoOperacionId == TipoOperacion.Ingreso).Sum(x => x.Monto);
            // Calculamos el balance de retiros para esta fecha filtrando y sumando las transacciones de tipo gasto.
            public decimal BalanceRetiros => Transacciones.Where(x => x.tipoOperacionId == TipoOperacion.Gasto).Sum(x => x.Monto);


        }
    }
}
