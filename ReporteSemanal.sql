use ManejoPresupuesto
/*
se declaran tres variables que controlan el rango de fechas y el ID del usuario cuyas transacciones se quieren analizar


*/
DECLARE @fechaInicio date = '2021-10-02';
DECLARE @fechaFin date = '2021-11-02';
DECLARE @usuarioId int = 1;
/*
Calcula la diferencia en días entre la FechaTransaccion y @fechaInicio 
Al dividir la diferencia en días por 7, se obtienen las semanas transcurridas desde la fecha de inicio.
Se suma 1 para numerar las semanas desde 1 (en lugar de comenzar desde 0).
-La función de agregación SUM suma los montos de todas las transacciones para cada semana. El campo resultante se nombra como Monto.
-Esta columna corresponde al tipo de operación de la transacción, que se obtiene de la tabla Categorias. 
Representa si es un ingreso o un gasto
-La clausula WHERE filtra las transacciones que:
Pertenecen al usuario con el ID almacenado en @usuarioId
Se encuentran dentro del rango de fechas definido por @fechaInicio y @fechaFin. La condición BETWEEN incluye tanto el @fechaInicio como el @fechaFin.
-Se GROUP BY para agrupar los resultados por:
Semana: Calculada mediante DATEDIFF y TipoOperacionId
Esto asegura que las transacciones se agrupen primero por semana y luego por tipo de operación, para sumar los montos de cada grupo.
*/
SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana, SUM(Monto) as Monto, cat.TipoOperacionId 
FROM Transacciones tr 
INNER JOIN Categorias cat 
ON cat.id = tr.CategoriaId 
WHERE tr.UsuarioId = @usuarioId AND tr.FechaTransaccion BETWEEN @fechaInicio AND @fechaFin 
GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId