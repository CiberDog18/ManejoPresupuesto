using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public class RepositorioCategorias : IRepositorioCategorias
    {
        /* Constructor del RepositorioCategorias para inicializar la cadena de conexión obtenida de la configuración */
        #region Constructor del RepositorioCategorias
        private readonly string connectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion


        /* Método para crear una nueva categoría en la base de datos, asignando el ID generado */
        #region Crear Categoría

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                Values (@Nombre, @TipoOperacionId, @UsuarioId);

                SELECT SCOPE_IDENTITY();", categoria);

            categoria.id = id;

        }
        #endregion

        /* Método para obtener todas las categorías de un usuario específico desde la base de datos */
        #region Obtener Categorías por Usuario

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@$"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId 
            ORDER BY Nombre 
            OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY;",
                new { usuarioId });
        }
        #endregion

        /* Método para obtener las categorías de un usuario específico filtradas por tipo de operación */
        #region Obtener Categorías por Usuario y Tipo de Operación

        public async Task<IEnumerable<Categoria>> ObtenerIdxTipoOpId(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId AND TipoOperacionId = @tipoOperacionId",
                new { usuarioId, tipoOperacionId });
        }
        #endregion

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId", new {usuarioId});
        }

        /* Método para obtener una categoría específica de un usuario por su ID */
        #region Obtener Categoría por ID

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                       @"Select * FROM Categorias WHERE id = @id AND UsuarioId = @UsuarioId;",
                       new { id, usuarioId });
        }
        #endregion

        /* Método para actualizar una categoría existente en la base de datos */
        #region Actualizar Categoría

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId WHERE id = @id;", categoria);
        }
        #endregion

        /* Método para eliminar una categoría de la base de datos por su ID */
        #region Borrar Categoría
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Categorias WHERE id = @id;", new { id });
        }
        #endregion





    }
}
