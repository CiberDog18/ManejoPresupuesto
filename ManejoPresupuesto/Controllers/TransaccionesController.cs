using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        /* Declaración de dependencias utilizadas por el controlador para gestionar usuarios, cuentas, categorías, transacciones y reportes */
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;

        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServicioUsuarios _servicioUsuarios, IRepositorioCuentas _repositorioCuentas, IRepositorioCategorias _repositorioCategorias,
            IRepositorioTransacciones _repositorioTransacciones, IMapper mapper, IServicioReportes _servicioReportes)
        {
            this.servicioUsuarios = _servicioUsuarios;
            this.repositorioCuentas = _repositorioCuentas;
            this.repositorioCategorias = _repositorioCategorias;
            this.repositorioTransacciones = _repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = _servicioReportes;
        }

        /* Método para obtener y organizar las transacciones semanales de un usuario específico basado en el mes y año proporcionados */
        #region Reporte Semanal de Transacciones
        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana =
                await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);
            // Agrupar las transacciones por la propiedad Semana, y para cada grupo se selecciona
            // el monto del primer ingreso y gasto encontrado para esa semana.
            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x =>
            new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,

                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),

                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            // Verifica si el año o el mes son inválidos (0), y si es así, asigna el mes y año actuales.
            if (año == 0 || mes == 0)
            {
                var hoy = DateTime.Today; 
                año = hoy.Year; 
                mes = hoy.Month; 
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            // Genera una lista de los días del mes desde el 1 hasta el último día del mes.
            // Se usa Enumerable.Range para crear una secuencia de días.
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            // Divide la lista de días en segmentos de 7 días (cada segmento representa una semana).
            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            // Itera sobre cada semana (segmento de días) del mes.
            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;// Calcula el número de semana (inicia desde 1).
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());// Define las fechas de inicio y fin de la semana actual.
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);// Busca si ya existe un grupo para esa semana en el reporte.
                // Si no existe información para esa semana, crea un nuevo grupo.
                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio, // Asigna la fecha de inicio de la semana.
                        FechaFin = fechaFin // Asigna la fecha de fin de la semana.
                    });
                }
                else // Si ya existe, solo actualiza las fechas de inicio y fin.
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }

            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            // Crea un modelo para la vista que contendrá las transacciones semanales y la fecha de referencia.
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }
        #endregion

        /* Método para obtener y organizar las transacciones mensuales de un usuario específico basado en el año proporcionado */
        #region Método Mensual - Reporte Mensual de Transacciones

        public async Task<IActionResult> Mensual(int año)
        {
            // Obtiene el ID del usuario actual que está autenticado en la aplicación.
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            // Verifica si el año es 0 (valor no válido) y si es así, se asigna el año actual.
            if (año == 0)
            {
                año = DateTime.Today.Year;  // Asigna el año actual si no se proporciona uno.
            }
            // Llama al repositorio de transacciones para obtener las transacciones del usuario agrupadas por mes.
            // Se pasa el usuarioId y el año como parámetros.
            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);
            // Agrupa las transacciones obtenidas por mes. Para cada mes, selecciona las transacciones
            // según el tipo de operación (Ingreso o Gasto) y calcula el primer ingreso y gasto del mes.
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x =>
            new ResultadoObtenerPorMes()
            {
                Mes = x.Key,// Asigna el mes como la clave del grupo.
                // Filtra las transacciones del grupo que sean de tipo "Ingreso" y selecciona el primer monto.
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                // Filtra las transacciones del grupo que sean de tipo "Gasto" y selecciona el primer monto.
                Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();


            // Convierte el resultado a una lista.
            // Este bucle itera sobre los meses del año (1 al 12) para asegurarse de que cada mes tenga
            // una entrada en la lista de transacciones agrupadas.
            for (int mes = 1; mes <= 12; mes++)
            {
                // Busca si ya existe una transacción para el mes actual en la lista agrupada.
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                // Crea un objeto DateTime para el primer día del mes actual.
                var fechaReferencia = new DateTime(año, mes, 1);
                // Si no hay transacciones registradas para este mes, añade una nueva entrada con la fecha de referencia.
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes, // Asigna el mes correspondiente.
                        FechaReferencia = fechaReferencia // Asigna la fecha de referencia para ese mes.
                    });
                }
                else
                {
                    // Si ya existe una transacción, simplemente actualiza la fecha de referencia.
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            // Ordena las transacciones agrupadas por mes en orden descendente (de mes más reciente a más antiguo).
            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            // Crea un modelo de tipo ReporteMensualViewModel que será utilizado para renderizar la vista.
            var modelo = new ReporteMensualViewModel();
            modelo.Año = año; // Asigna el año actual al modelo.
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            // Devuelve la vista con el modelo que contiene las transacciones del reporte mensual.
            return View(modelo);
        }
        #endregion



        /* Acción para renderizar la vista de exportación en Excel */
        #region Acción de vista para Exportar en Excel
        public IActionResult Excel()
        {
            return View();
        }
        #endregion

        /* Acción para exportar transacciones de un mes específico a un archivo Excel */
        #region Exportación de Excel por Mes
        [HttpGet] 
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {

            var fechaInicio = new DateTime(año, mes, 1);
            
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);


            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var trasacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario
                {
                    UsuarioId = usuarioId, 
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });
           
            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, trasacciones);
        }
        #endregion


        /* Acción para exportar todas las transacciones de un año específico a un archivo Excel */
        #region Exportación de Excel por Año

        [HttpGet] 
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
           
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId=usuarioId, 
                    FechaInicio = fechaInicio, 
                    FechaFin = fechaFin 

                });

            var nombreArchvo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchvo, transacciones);


        }
        #endregion

        /* Acción para exportar todas las transacciones sin límite de fechas a un archivo Excel */
        #region Exportación de Excel con Todas las Transacciones
        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo() 
        {
           
            var fechaInicio = DateTime.Today.AddYears(-100);

            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
           
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario(){
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio, 
                    FechaFin = fechaFin 
                });

            var nombreArchvo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
            return GenerarExcel(nombreArchvo , transacciones);
        }
        #endregion

        /* Método para generar un archivo Excel con las transacciones y enviarlo como descarga */
        #region Generación y Exportación de Archivo Excel
        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
              new DataColumn("Fecha"), 
               new DataColumn("Cuenta"), 
               new DataColumn("Categoria"), 
                 new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

     
            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.tipoOperacionId);
            }

            // Generación del archivo Excel con ClosedXML y almacenamiento en memoria
            using (XLWorkbook wb = new XLWorkbook())
            {
                //  Se crea una hoja de cálculo en el archivo Excel utilizando los datos del DataTable. Cada 
                // fila en el DataTable se convierte en una fila en el archivo Excel.
                wb.Worksheets.Add(dataTable);

                //  Se utiliza un MemoryStream para almacenar temporalmente el archivo Excel en memoria.
                using(MemoryStream stream = new MemoryStream())
                {
                    //  El archivo Excel se guarda en el stream.
                    wb.SaveAs(stream);
                    // 14. Se devuelve el archivo Excel al cliente como una respuesta HTTP.
                    return File(stream.ToArray(),
                         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Tipo de archivo Excel.
                         nombreArchivo 
                        );           

                }
            }

        }
        #endregion


        /* Acción para mostrar la vista del calendario */
        #region Acción de Vista para el Calendario

        public IActionResult Calendario()
        {
            return View();
        }
        #endregion

        /* Acción para obtener las transacciones de un usuario en un rango de fechas y convertirlas en eventos de calendario */
        #region Obtener Transacciones para el Calendario
        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
               new ParametroObtenerTransaccionesPorUsuario
               {
                   UsuarioId = usuarioId,
                   FechaInicio = start,
                   FechaFin = end
               });
            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("F1"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.tipoOperacionId == TipoOperacion.Gasto) ? "Red" : "Green"

            });
            return Json(eventosCalendario);
        }
        #endregion


        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(
               new ParametroObtenerTransaccionesPorUsuario
               {
                   UsuarioId = usuarioId,
                   FechaInicio = fecha,
                   FechaFin = fecha
               });

            return Json(transacciones);
        }


        /* Acción para mostrar el formulario de creación de transacciones, cargando las cuentas y categorías del usuario */
        #region Acción de Vista para Crear Transacción
        //Se usa principalmente para preparar y mostrar una vista para crear una nueva transacción, cargando previamente las cuentas del usuario actual.
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.tipoOperacionId);
            return View(modelo);
        }
        #endregion

        /* Acción para procesar el formulario de creación de transacción; valida datos y guarda la transacción en la base de datos */
        #region Procesar Creación de Transacción

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.tipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;

            if (modelo.tipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }
        #endregion

        // Método que obtiene las cuentas de un usuario y las prepara para su selección en una lista desplegable
        #region Obtener Cuentas para Lista de Selección
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
        }
        #endregion



        // Acción que recibe el tipo de operación (Ingreso o Gasto) y retorna las categorías asociadas para el usuario
        #region Listar Categorías por Tipo de Operación

        [HttpPost]
        public async Task<IActionResult> ListarCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
        #endregion

        // Método que obtiene las categorías de un usuario según el tipo de operación, para mostrar en una lista de selección
        #region Obtener Categorías para Lista de Selección
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.ObtenerIdxTipoOpId(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
        }
        #endregion


        // Acción principal para mostrar el índice de transacciones detalladas de un usuario, filtradas por mes y año
        #region Mostrar Reporte Detallado de Transacciones
        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }
        #endregion

        // Acción para mostrar el formulario de edición de una transacción específica, cargando datos de cuentas y categorías del usuario
        #region Acción de Vista para Editar Transacción
        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            // Obtiene el ID del usuario actual autenticado a través del servicio de usuarios.
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            // Obtiene la transacción correspondiente al ID proporcionado, validando que pertenezca al usuario actual.
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            // Si no se encuentra la transacción o no pertenece al usuario, redirige a la página "No Encontrado".
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Mapea la entidad 'transaccion' a un modelo de vista 'TransaccionActualizacionViewModel'.
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            // Guarda el monto original de la transacción en 'MontoAnterior' para futuras comparaciones.
            modelo.MontoAnterior = modelo.Monto;
            // Si la transacción es un gasto, el monto anterior se convierte en negativo.
            if (modelo.tipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            // Almacena el ID de la cuenta actual en el modelo para comparaciones al editar.
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            // Carga las categorías asociadas al tipo de operación (Ingreso o Gasto) del usuario actual.
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.tipoOperacionId);
            // Carga las cuentas del usuario para poder asignar la transacción a una cuenta específica.
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            // Asigna la URL de retorno al modelo para redirigir al usuario después de la edición.
            modelo.UrlRetorno = urlRetorno;
            // Devuelve la vista con el modelo que contiene toda la información necesaria para la edición.
            return View(modelo);
        }
        #endregion

        // Acción para procesar el formulario de edición de la transacción, validando y actualizando la transacción en la base de datos
        #region Procesar Edición de Transacción

        //Este método maneja la solicitud POST cuando el usuario envía los cambios realizados en el formulario de edición.
        //Aquí es donde se actualiza la transacción en la base de datos.
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            // Si los datos del modelo no son válidos, recarga las cuentas y categorías para mostrarlas en la vista de edición.
            if (!ModelState.IsValid)
            {
                //estamos obteniendo las cuentas y categorías del usuario para mostrarlas en la página de edición.
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.tipoOperacionId);
                return View(modelo);
            }
            // Verifica que la cuenta seleccionada por el usuario existe y pertenece al usuario autenticado.
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Verifica que la categoría seleccionada por el usuario existe y pertenece al usuario autenticado.
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Mapea el modelo de vista a la entidad 'Transaccion' para actualizarla en la base de datos
            var transaccion = mapper.Map<Transaccion>(modelo);
            // Si la transacción es un gasto, multiplica el monto por -1 para reflejar correctamente el gasto en la base de datos.
            if (modelo.tipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }
            // Actualiza la transacción en el repositorio de transacciones, incluyendo el monto anterior y la cuenta anterior.
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            // Si no se especificó una URL de retorno, redirige a la página principal de transacciones.
            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Si se proporcionó una URL de retorno, redirige a dicha URL.
                return LocalRedirect(modelo.UrlRetorno);
            }

        }
        #endregion

        // Acción para eliminar una transacción específica, validando que pertenezca al usuario y redirigiendo según la URL de retorno
        #region Acción para Borrar Transacción

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }
        #endregion



    }


}
