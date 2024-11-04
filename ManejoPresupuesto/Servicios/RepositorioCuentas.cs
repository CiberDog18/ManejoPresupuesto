using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;

namespace ManejoPresupuesto.Servicios
{
    public class RepositorioCuentas : IRepositorioCuentas
    {
        /* Constructor del RepositorioCategorias para inicializar la cadena de conexión obtenida de la configuración */
        #region Constructor del RepositorioCategorias

        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion

        /* Método para crear una nueva cuenta en la base de datos y asignar el ID generado */
        #region Crear Cuenta

        public async Task Crear(Cuenta cuenta) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>( 
                @"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance) VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.id = id; 

        }
        #endregion

        /* Método para buscar todas las cuentas de un usuario específico, incluyendo su tipo de cuenta */
        #region Buscar Cuentas por Usuario

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            //// Ejecuta una consulta SQL que devuelve una colección de objetos Cuenta
            var sql = @"SELECT c.id, c.Nombre, c.Balance, tc.Nombre AS TipoCuenta FROM Cuentas c 
                INNER JOIN TiposCuentas tc 
                ON tc.id = c.TipoCuentaId 
                WHERE tc.UsuarioId = @UsuarioId
                ORDER BY tc.Orden;";
            var cuentas = await connection.QueryAsync<Cuenta>(sql, new { usuarioId });
            // Itera sobre cada cuenta obtenida.
            foreach (var cuenta in cuentas)
            {
                // Imprime detalles de la cuenta en la consola.
                Console.WriteLine($"Cuenta ID: {cuenta.id}, Nombre: {cuenta.Nombre}, TipoCuenta: {cuenta.TipoCuenta}");
                cuenta.TipoCuenta = cuenta.TipoCuenta ?? "SinNombre"; // Asigna un valor predeterminado si es null
            }
            // Devuelve la colección de cuentas.
            return cuentas;
        }
        #endregion

        /* Método para obtener una cuenta específica de un usuario por su ID */
        #region Obtener Cuenta por ID

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"SELECT Cuentas.id, Cuentas.Nombre, Balance, Descripcion, Cuentas.TipoCuentaId
                FROM Cuentas
                INNER JOIN TiposCuentas tc
                ON tc.id = Cuentas.TipoCuentaId
                WHERE tc.UsuarioId = @UsuarioId AND Cuentas.id = @id ;", new { id, usuarioId });

        }
        #endregion

        /* Método para actualizar los datos de una cuenta existente en la base de datos */
        #region Actualizar Cuenta

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas 
                SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId 
                WHERE id = @id;", cuenta);
        }
        #endregion

        /* Método para eliminar una cuenta de la base de datos por su ID */
        #region Borrar Cuenta

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Cuentas WHERE id = @id", new { id });
        }
        #endregion



    }

}
