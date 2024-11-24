namespace ManejoPresupuesto.Models
{
    /// Clase para manejar la lógica de paginación, incluyendo número de página, 
    /// registros por página y el cálculo de registros a saltar.
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        public int recordXPagina = 10;
        public readonly int cantidadMaximaRecordsPorPagina = 50;

        public int RecordsPorPagina
        {
            get
            {
                return recordXPagina;
            }
            set
            {
                recordXPagina = (value > cantidadMaximaRecordsPorPagina) ?
                        cantidadMaximaRecordsPorPagina : value;
            }
        }

        public int RecordsASaltar => recordXPagina * (Pagina - 1);

    }
}
