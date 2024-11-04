using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {

        /* Declaración de dependencias utilizadas por el controlador para gestionar tipos de cuentas, usuarios, transacciones y reportes */
        #region Dependencias del controlador
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;
        /* Constructor del CuentasController para la inyección de dependencias necesarias para las operaciones de cuentas */
        public CuentasController(IRepositorioTiposCuentas _repositorioTiposCuentas, IServicioUsuarios _servicioUsuarios,
            IRepositorioCuentas _repositorioCuentas, IMapper _mapper, IRepositorioTransacciones _repositorioTransacciones,
            IServicioReportes _servicioReportes)
        {
            this.repositorioTiposCuentas = _repositorioTiposCuentas;
            this.servicioUsuarios = _servicioUsuarios;
            this.repositorioCuentas = _repositorioCuentas;
            this.mapper = _mapper;
            this.repositorioTransacciones = _repositorioTransacciones;
            this.servicioReportes = _servicioReportes;
        }
        #endregion

        #region Crear Cuenta
        // Acción para mostrar el formulario de creación de una nueva cuenta, cargando los tipos de cuentas disponibles
        #region Acción de Vista para Crear Cuenta

        [HttpGet]
        public async Task<IActionResult> Crear() // Método asíncrono que maneja la creación de una nueva cuenta
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId(); // Obtiene el ID del usuario actual
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId); // Obtiene los tipos de cuentas para el usuario actual de forma asíncrona
            var modelo = new CuentaCreacionViewModel(); // Crea una nueva instancia del modelo de creación de cuenta
            // Convierte los tipos de cuentas en elementos seleccionables para una lista desplegable
            modelo.TiposCuentas = tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
            return View(modelo);// Devuelve la vista con el modelo de datos
        }
        #endregion

        // Acción para procesar el formulario de creación de cuenta, validando datos y guardando en la base de datos
        #region Procesar Creación de Cuenta

        //Este código se utiliza para procesar y validar los datos del formulario de creación de cuentas,
        //manejar errores y finalmente agregar la nueva cuenta a la base de datos si todos los datos son correctos.
        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)  // Método asíncrono que maneja la creación de una nueva cuenta
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerXid(cuenta.TipoCuentaId, usuarioId); // Obtiene el tipo de cuenta especificado para el usuario actual

            if (tiposCuentas is null) // Si el tipo de cuenta no se encuentra
            {
                return RedirectToAction("NoEncontrado", "Home"); // Redirige a la acción "NoEncontrado" en el controlador "Home"

            }
            if (!ModelState.IsValid) // Si el modelo no es válido
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId); // Obtiene los tipos de cuentas del usuario y los asigna al modelo
                return View(cuenta);// Devuelve la vista con el modelo actual para corregir errores
            }

            await repositorioCuentas.Crear(cuenta); // Crea la nueva cuenta en el repositorio
            return RedirectToAction("Index"); // Redirige a la acción "Index" del controlador actual
        }
        #endregion

        #endregion


        // Acción para mostrar el índice de cuentas, agrupando las cuentas del usuario por tipo de cuenta
        #region Vista para Índice de Cuentas
        //Este código se utiliza para preparar y mostrar los datos en la página de índice de cuentas,
        //proporcionando una visión general de las cuentas del usuario agrupadas por tipo de cuenta.
        public async Task<IActionResult> Index()  // Método asíncrono que maneja la vista de la página de índice de cuentas
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId); // Obtiene las cuentas del usuario con sus tipos de cuenta asociados

            var modelo = cuentasConTipoCuenta // Crea el modelo de vista para la página de índice de cuentas
                .GroupBy(x => x.TipoCuenta) // Agrupa las cuentas por tipo de cuenta
                .Select(grupo => new IndiceCuentasViewModel  // Crea un objeto IndiceCuentasViewModel para cada grupo de cuentas
                {
                    TipoCuenta = grupo.Key, // Establece el tipo de cuenta para el modelo
                    Cuentas = grupo.AsEnumerable() // Asigna las cuentas correspondientes al tipo de cuenta
                }).ToList();

            return View(modelo); // Devuelve la vista con el modelo de datos para mostrar en la página de índice de cuentas
        }
        #endregion

        #region Editar Cuentas
        // Acción para mostrar el formulario de edición de una cuenta específica, cargando datos de tipos de cuentas del usuario
        #region Vista para Editar Cuenta

        // Método asíncrono que edita una cuenta y devuelve una vista.
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Mapea la cuenta a un modelo de vista de creación de cuenta.
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);
            // Obtiene los tipos de cuentas para el usuario actual.
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            // Devuelve la vista con el modelo de datos.
            return View(modelo);

        }
        #endregion

        // Acción para procesar el formulario de edición de cuenta, validando datos y guardando cambios en la base de datos
        #region Procesar Edición de Cuenta

        //El método GET prepara y muestra la página de edición. El método POST procesa y guarda los cambios realizados en la cuenta.
        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Obtiene el tipo de cuenta por su id y el id del usuario.
            var tipoCuenta = await repositorioTiposCuentas.ObtenerXid(cuentaEditar.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }
        #endregion

        #endregion

        #region BorrarCuentas
        // Acción para mostrar la confirmación de borrado de una cuenta específica
        #region Vista para Confirmación de Borrado de Cuenta

        // método asíncrono se utiliza para preparar y mostrar una vista que permite al usuario confirmar la eliminación de una cuenta. 
        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }
        #endregion

        // Acción para procesar el borrado de una cuenta, eliminándola de la base de datos
        #region Procesar Borrado de Cuenta

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
        #endregion
        #endregion


        // Método privado para obtener los tipos de cuentas del usuario y prepararlos como elementos de una lista desplegable
        #region Obtener Tipos de Cuentas para Lista de Selección
        //Este metodo es útil en situaciones donde se necesita obtener y preparar los tipos de cuentas para su presentación en la interfaz de usuario,
        //como en formularios de creación o edición de cuentas, donde se puede necesitar una lista desplegable para seleccionar el tipo de cuenta asociado.
        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId) // Método privado asíncrono que obtiene los tipos de cuentas del usuario
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId); 
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.id.ToString()));
        }
        #endregion

        // Acción para mostrar un reporte detallado de transacciones para una cuenta específica en un rango de fechas
        #region Generar Reporte Detallado de Transacciones por Cuenta

        //Generar y mostrar un reporte detallado de transacciones para una cuenta específica.
        //Este método obtiene y muestra un reporte detallado de transacciones para una cuenta específica dentro de un rango de fechas.
        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            ViewBag.Cuenta = cuenta.Nombre;
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, id, mes, año, ViewBag);
            return View(modelo);

        }
        #endregion

    }

}
