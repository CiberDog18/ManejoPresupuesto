namespace ManejoPresupuesto.Models
{
    public class ParametroObtenerTransaccionesPorUsuario
    {
        // Propiedad pública que almacena el ID del usuario. Es utilizada para identificar de qué usuario se quieren obtener las transacciones.
        public int UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
