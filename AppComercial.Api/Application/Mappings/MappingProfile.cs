using AppComercial.Api.Application.DTOs;
using AppComercial.Api.Models;
using AutoMapper;

namespace AppComercial.Api.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AdmProductos, ProductoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDPRODUCTO))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOPRODUCTO))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.CNOMBREPRODUCTO))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.CDESCRIPCIONPRODUCTO))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => (decimal)src.CPRECIO1))
            .ForMember(dest => dest.UnidadMedidaId, opt => opt.MapFrom(src => src.CIDUNIDADBASE))
            .ForMember(dest => dest.UnidadMedidaNombre, opt => opt.MapFrom(src => string.Empty)) // TODO: Map from related table
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.CSTATUSPRODUCTO == 1));

        CreateMap<AdmClientes, ClienteDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDCLIENTEPROVEEDOR))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOCLIENTE))
            .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.CRAZONSOCIAL))
            .ForMember(dest => dest.RFC, opt => opt.MapFrom(src => src.CRFC))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CEMAIL1))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.CESTATUS == 1));
    }
}