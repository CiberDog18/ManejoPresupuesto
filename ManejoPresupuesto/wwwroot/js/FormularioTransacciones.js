// Función que se ejecuta cuando se carga el formulario
// Recibe como parámetro la URL para obtener las categorías
function inicializarFormularioTransacciones(urlObtenerCategorias) {
    // Cuando el valor del elemento con id "tipoOperacionId" cambie, ejecuta la función.
    $("#tipoOperacionId").change(async function () {
        // Obtén el valor seleccionado del elemento "tipoOperacionId".
        const valorSeleccionado = $(this).val();
        // Envía una solicitud POST al servidor con el valor seleccionado
        const resouesta = await fetch(urlObtenerCategorias, {
            method: 'POST', // Usa el método POST para enviar datos.
            body: valorSeleccionado, // Envía el valor seleccionado como el cuerpo de la solicitud.
            headers: {
                'Content-Type': 'application/json' // Especifica que el contenido es JSON
            }
        });
        // Espera la respuesta del servidor y conviértela a JSON.
        const json = await resouesta.json();
        console.log(json);
        // Crea una lista de opciones HTML para un elemento <select> a partir del JSON recibido.
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        // Actualiza el contenido del elemento con id "CategoriaId" con las nuevas opciones.
        $("#CategoriaId").html(opciones);
    })
}