namespace ManejoPresupuesto.Models
{
    // Clase que representa el modelo de datos utilizado para el reporte semanal en la vista.
    public class ReporteSemanalViewModel
    {
        // Propiedad calculada que suma todos los ingresos de las transacciones por semana.
        // Utiliza LINQ y la función Sum() para sumar los valores de la propiedad "Ingresos"     de cada semana.
        public decimal Ingresos => TransaccionPorSemana.Sum(x => x.Ingresos);
        // Propiedad calculada que suma todos los gastos de las transacciones por semana.
        // Al igual que en Ingresos, usa LINQ con Sum() para calcular el total de los gastos.
        public decimal Gastos => TransaccionPorSemana.Sum(x => x.Gastos);
        // Propiedad calculada que devuelve el total neto, es decir, la diferencia entre los ingresos y los gastos.
        // Total = Ingresos - Gastos. Esto representa el balance final.
        public decimal Total => Ingresos - Gastos;
        // Fecha de referencia del reporte, probablemente el primer día del mes en cuestión.
        public DateTime FechaReferencia { get; set; }

        // Colección de transacciones agrupadas por semana, donde cada elemento es del tipo ResultadoObtenerPorSemana.
        // Esta colección contiene las transacciones semanales (ingresos, gastos, fechas de inicio/fin).
        public IEnumerable<ResultadoObtenerPorSemana> TransaccionPorSemana { get; set; }

    }
}
