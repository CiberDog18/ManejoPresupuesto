using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicios
{
    public class ServicioReportes : IServicioReportes
    {
        /* Declaración de dependencias utilizadas por el servicio, incluyendo el repositorio de transacciones y el contexto HTTP */
        #region Dependencias del servicio

        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReportes(IRepositorioTransacciones _repositorioTransacciones, IHttpContextAccessor _httpContextAccessor)
        {
            this.repositorioTransacciones = _repositorioTransacciones;
            this.httpContext = _httpContextAccessor.HttpContext;

        }
        #endregion

        /* Método para obtener un reporte detallado de transacciones para un usuario en un mes y año específicos */
        #region Obtener Reporte Detallado de Transacciones

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);
            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);
            var modelo = GenerarReporeTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            return modelo;

        }
        #endregion

        /* Método para obtener un reporte detallado de transacciones de una cuenta específica para un usuario en un mes y año */
        #region Obtener Reporte Detallado de Transacciones por Cuenta

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId, // ID del usuario autenticado.
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);
            var modelo = GenerarReporeTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            return modelo;

        }
        #endregion

        /* Método para obtener un reporte semanal de transacciones para un usuario en un mes y año específicos */
        #region Obtener Reporte Semanal de Transacciones

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioId, 
            int mes, int año, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            var modelo = await repositorioTransacciones.ObtenerPorSemana(parametro);
            return modelo;
        }
        #endregion


        /* Método para asignar valores de navegación (mes anterior/posterior) al ViewBag */
        #region Asignar Valores al ViewBag

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            // Calcular el mes y año anterior para facilitar la navegación en la vista.
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            // Calcular el mes y año posterior para facilitar la navegación en la vista.
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.UrlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }
        #endregion

        /* Método para generar un modelo detallado de reporte de transacciones, agrupadas por fecha */
        #region Generar Reporte Detallado de Transacciones

        private static ReporteTransaccionesDetalladas GenerarReporeTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetalladas();
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion) // Ordenar por fecha (más reciente primero)
                .GroupBy(x => x.FechaTransaccion) // Agrupar por fecha de transacción.
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key, // La fecha de las transacciones agrupadas.
                    Transacciones = grupo.AsEnumerable() // Las transacciones que pertenecen a esa fecha.
                });
            // Asignar las transacciones agrupadas al modelo.
            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            // Asignar las fechas de inicio y fin al modelo.
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }
        #endregion

        /* Método para generar las fechas de inicio y fin de un mes y año específicos, o del mes actual si no se proporciona */
        #region Generar Fechas de Inicio y Fin

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }
        #endregion

    }
}
