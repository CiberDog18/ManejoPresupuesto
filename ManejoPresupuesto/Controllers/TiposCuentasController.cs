using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {

        /* Declaración de dependencias necesarias para el controlador, incluyendo el repositorio de tipos de cuentas y el servicio de usuarios */
        #region Dependencias del controlador
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuenta, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuenta;
            this.servicioUsuarios = servicioUsuarios;
        }
        #endregion

        /* Método de acción para mostrar el índice de tipos de cuentas, obteniendo y pasando los tipos de cuentas del usuario a la vista */
        #region Vista para Índice de Tipos de Cuentas

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            // Obtiene todos los 'TipoCuenta' para el usuario con ID 1 de la base de datos.
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            // Devuelve la vista 'Index', pasando la lista de 'TipoCuenta' como modelo.
            return View(tiposCuentas);
        }
        #endregion

        #region Crear Tipo de Cuenta
        // Acción para mostrar el formulario de creación de un nuevo tipo de cuenta
        #region Vista para Crear Tipo de Cuenta
        public IActionResult Crear()
        {
            return View();
        }
        #endregion

        // Acción para procesar el formulario de creación de un tipo de cuenta, validando datos y guardando en la base de datos
        #region Procesar Creación de Tipo de Cuenta

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta) // Método de acción para crear un nuevo 'TipoCuenta'.
        {
            // Verifica si el modelo es válido.
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a la vista con el modelo actual (que contiene los errores de validación).
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();
  

            // Verifica si ya existe un 'TipoCuenta' con el mismo nombre para el mismo usuario.
            var yaExisteTipoCuenta =
                await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            // Si ya existe un 'TipoCuenta' con el mismo nombre...
            if (yaExisteTipoCuenta)
            {
                // Agrega un error al modelo indicando que el nombre ya existe.
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");
                // Vuelve a la vista con el modelo actual (que contiene el error).
                return View(tipoCuenta);
            }

            // Si no hay errores, crea el nuevo 'TipoCuenta' en la base de datos.
            await repositorioTiposCuentas.Crear(tipoCuenta);
            // Redirige al usuario a la acción 'Index' del controlador.
            return RedirectToAction("Index");
        }
        #endregion
        #endregion

        #region Editar Tipo de Cuenta
        // Acción para mostrar el formulario de edición de un tipo de cuenta específico
        #region Vista para Editar Tipo de Cuenta
        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerXid(id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }
        #endregion

        // Acción para procesar el formulario de edición de un tipo de cuenta, validando datos y actualizando en la base de datos
        #region Procesar Edición de Tipo de Cuenta

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerXid(tipoCuenta.id, usuarioId);
            if (tipoCuentaExiste == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }
        #endregion
        #endregion


        // Acción para verificar si ya existe un tipo de cuenta con el nombre proporcionado para el usuario
        #region Verificar Existencia de Tipo de Cuenta por Nombre
        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = 1;
            // Verifica si ya existe un 'TipoCuenta' con el nombre proporcionado para el usuario con ID 1.
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            // Si ya existe un 'TipoCuenta' con el nombre proporcionado...
            if (yaExisteTipoCuenta)
            {

                // Devuelve una respuesta JSON indicando que el nombre ya existe.
                return Json($"El nombre {nombre} ya existe.");
            }
            // Si no existe un 'TipoCuenta' con el nombre proporcionado, devuelve una respuesta JSON con 'true'.
            return Json(true);
        }
        #endregion


        // Acción para mostrar la vista de confirmación de eliminación de un tipo de cuenta específico
        #region Acción de Vista para Confirmación de Eliminación de Tipo de Cuenta
        public async Task<ActionResult> Eliminar(int id)
        {
            // Obtiene el ID del usuario actualmente autenticado.
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            // Intenta obtener el 'TipoCuenta' con el ID proporcionado que pertenece al usuario actual.
            var tipoCuenta = await repositorioTiposCuentas.ObtenerXid(id, usuarioId);
            // Si no se encuentra ningún 'TipoCuenta' con el ID proporcionado para el usuario actual...
            if (tipoCuenta == null)
            {
                // Redirige al usuario a la vista 'NoEncontrado' del controlador 'Home'.
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Si se encuentra un 'TipoCuenta', devuelve la vista de eliminación, pasando el 'TipoCuenta' como modelo.
            return View(tipoCuenta);
        }
        #endregion

        // Acción para procesar la eliminación de un tipo de cuenta, verificando que pertenezca al usuario.
        #region Procesar Eliminación de Tipo de Cuenta

        [HttpPost]
        public async Task<ActionResult> EliminarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerXid(id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Si se encuentra un 'TipoCuenta', lo elimina de la base de datos.
            await repositorioTiposCuentas.Eliminar(id);
            // Redirige al usuario a la vista 'Index' del controlador.
            return RedirectToAction("Index");
        }
        #endregion


        // Acción para actualizar el orden de los tipos de cuentas de un usuario
        #region Procesar Ordenamiento de Tipos de Cuentas

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var idsTipoCuenta = tiposCuentas.Select(x => x.id);

            var idsTiposCuentasNoPerteneceAlUsuario = ids.Except(idsTipoCuenta).ToList();

            if (idsTiposCuentasNoPerteneceAlUsuario.Count() > 0)
            {
                return Forbid();
            }
            var tipoCuentasOrdenados = ids.Select((valor, indice) => new TipoCuenta()
            {
                id = valor,
                Orden = indice + 1
            }).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tipoCuentasOrdenados);
            return Ok();
        }
        #endregion



    }
}
