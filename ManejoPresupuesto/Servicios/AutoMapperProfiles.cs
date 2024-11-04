using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicios
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Crea un mapeo entre la clase Cuenta y la clase CuentaCreacionViewModel.
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            // Configura la conversión entre TransaccionActualizacionViewModel y Transaccion, permitiendo la transformación de ida y vuelta.
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();
        }

    }
}
