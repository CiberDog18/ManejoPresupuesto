using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    /* Constructor del RepositorioTiposCuentas para inicializar la cadena de conexión obtenida de la configuración */
    #region Constructor del RepositorioTiposCuentas
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion

        /* Método para crear un nuevo tipo de cuenta en la base de datos utilizando un procedimiento almacenado */
        #region Crear Tipo de Cuenta

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("TiposCuentasInsertar", new {
            usuarioId = tipoCuenta.UsuarioId,
            nombre = tipoCuenta.Nombre
            }, commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.id = id;

        }
        #endregion

        /* Método para verificar si ya existe un tipo de cuenta con el mismo nombre para un usuario específico */
        #region Verificar Existencia de Tipo de Cuenta

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;", new { nombre, usuarioId });
            return existe == 1;

        }
        #endregion

        /* Método para obtener todos los tipos de cuenta de un usuario, ordenados */
        #region Obtener Tipos de Cuenta por Usuario

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return (await connection.QueryAsync<TipoCuenta>(@"SELECT id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId ORDER BY Orden;", new { usuarioId }));
        }
        #endregion


        /* Método para actualizar el nombre de un tipo de cuenta en la base de datos */
        #region Actualizar Tipo de Cuenta
        public async Task Actualizar (TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE id = @id;", tipoCuenta);
        }
        #endregion

        /* Método para obtener un tipo de cuenta específico por su ID y el ID del usuario */
        #region Obtener Tipo de Cuenta por ID

        public async Task<TipoCuenta> ObtenerXid(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT id, Nombre, Orden FROM TiposCuentas WHERE id = @id AND UsuarioId = @UsuarioId;", new { id, usuarioId });
        }
        #endregion

        /* Método para eliminar un tipo de cuenta de la base de datos por su ID */
        #region Eliminar Tipo de Cuenta

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE FROM TiposCuentas WHERE id = @id;", new { id });
        }
        #endregion

        /* Método para actualizar el orden de múltiples tipos de cuenta en la base de datos */
        #region Ordenar Tipos de Cuenta

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenar)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE id = @id;";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenar);

        }
        #endregion

    }
}
