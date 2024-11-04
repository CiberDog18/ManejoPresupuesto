using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManejoPresupuesto.Servicios
{
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        /* Constructor del RepositorioTransacciones para inicializar la cadena de conexión obtenida de la configuración */
        #region Constructor del RepositorioTransacciones
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion

        /* Método para crear una nueva transacción en la base de datos utilizando un procedimiento almacenado */
        #region Crear Transacción
        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                //Indica que el comando que se está ejecutando es un procedimiento almacenado en la base de datos, no una consulta SQL directa.
                commandType: System.Data.CommandType.StoredProcedure);
            transaccion.id = id;
        }
        #endregion

        /* Método para actualizar una transacción existente en la base de datos utilizando un procedimiento almacenado */
        #region Actualizar Transacción

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }
        #endregion

        /* Método para eliminar una transacción de la base de datos utilizando un procedimiento almacenado */
        #region Borrar Transacción

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
        #endregion

        /* Método para obtener una transacción específica por su ID y el ID del usuario, incluyendo el tipo de operación */
        #region Obtener Transacción por ID

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT Transacciones.*, cat.TipoOperacionId FROM Transacciones 
                    INNER JOIN Categorias cat ON cat.id = Transacciones.CategoriaId 
                    WHERE Transacciones.id = @id AND Transacciones.UsuarioId = @UsuarioId;",
                new { id, usuarioId });
        }
        #endregion

        /* Método para obtener transacciones de una cuenta específica de un usuario, filtradas por un rango de fechas */
        #region Obtener Transacciones por Cuenta

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta model)
        {
            using var connection = new SqlConnection(connectionString);
           
            return await connection.QueryAsync<Transaccion>(
                @"SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId FROM Transacciones t 
                    INNER JOIN Categorias c 
                    ON c.id = t.CategoriaId 
                    INNER JOIN Cuentas cu 
                    ON cu.id = t.CuentaId 
                    WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId 
                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;", model);
        }
        #endregion

        /* Método para obtener todas las transacciones de un usuario, filtradas por un rango de fechas */
        #region Obtener Transacciones por Usuario

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario model)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(
                @"SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId, t.Nota FROM Transacciones t 
                    INNER JOIN Categorias c 
                    ON c.id = t.CategoriaId 
                    INNER JOIN Cuentas cu 
                    ON cu.id = t.CuentaId 
                    WHERE t.UsuarioId = @UsuarioId 
                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY t.FechaTransaccion DESC;", model);
        }
        #endregion

        /* Método para obtener el monto total de las transacciones por semana para un usuario, agrupado por tipo de operación */
        #region Obtener Transacciones por Semana

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                    SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana, SUM(Monto) as Monto, cat.TipoOperacionId 
                    FROM Transacciones tr 
                    INNER JOIN Categorias cat 
                    ON cat.id = tr.CategoriaId 
                    WHERE tr.UsuarioId = @usuarioId AND tr.FechaTransaccion BETWEEN @fechaInicio AND @fechaFin 
                    GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId
                    "
            , modelo);
        }
        #endregion

        /* Método para obtener el monto total de las transacciones por mes para un usuario en un año específico, agrupado por tipo de operación */
        #region Obtener Transacciones por Mes

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
                        SELECT MONTH(FechaTransaccion) AS Mes, SUM(Monto) as Monto, cat.TipoOperacionId 
                        FROM Transacciones tr 
                        INNER JOIN Categorias cat 
                        ON cat.id = tr.CategoriaId 
                        WHERE tr.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @Año 
                        GROUP BY MONTH(FechaTransaccion), cat.TipoOperacionId", new {usuarioId, año});
        }
        #endregion


    }


}
