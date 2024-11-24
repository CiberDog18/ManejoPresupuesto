using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        /* Declaración de dependencias utilizadas por el controlador, incluyendo el repositorio de categorías y el servicio de usuarios */
        #region Dependencias del controlador

        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuarios servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias,
            IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuarios = servicioUsuarios;
        }
        #endregion

        /* Acción para mostrar el índice de categorías, obteniendo y pasando las categorías del usuario a la vista */
        #region Acción de Vista para Índice de Categorías

        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.Obtener(usuarioId, paginacion);
            var totalCategorias = await repositorioCategorias.Contar(usuarioId);
            var respuestaVM = new PaginacionRespuesta<Categoria>
            {
                Elementos = categorias,
                Pagina = paginacion.Pagina,
                RecordsXPagina = paginacion.recordXPagina,
                CantidadTotalRecords = totalCategorias,
                BaseURL = "/categorias"
            };
            return View(respuestaVM);
        }
        #endregion

        #region Crear Categoria
        // Acción para mostrar el formulario de creación de una nueva categoría
        #region Acción de Vista para Crear Categoría
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }
        #endregion

        // Acción para procesar el formulario de creación de una categoría, validando datos y guardando en la base de datos
        #region Procesar Creación de Categoría

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = servicioUsuarios.ObtenerUsuarioId(); // Debe devolver 1
            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");

        }
        #endregion
        #endregion


        #region Editar Categoria
        // Acción para mostrar el formulario de edición de una categoría específica
        #region Acción de Vista para Editar Categoría

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }
        #endregion

        // Acción para procesar el formulario de edición de una categoría, validando datos y actualizando en la base de datos
        #region Procesar Edición de Categoría
        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(categoriaEditar.id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCategorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }
        #endregion
        #endregion


        #region BorrarCategoria
        // Acción para mostrar la vista de confirmación de borrado de una categoría específica
        #region Vista para Confirmación de Borrado de Categoría
        public async Task<IActionResult> Borrar (int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);

        }
        #endregion

        // Acción para procesar la eliminación de una categoría, verificando que pertenezca al usuario
        #region Procesar Borrado de Categoría
        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCategorias.Borrar(id);
            return RedirectToAction("Index");
        }
        #endregion
        #endregion


    }
}
