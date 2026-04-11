using AppComercial.Application.DTOs;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AutoMapper;

namespace AppComercial.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AdmProductos, ProductoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDPRODUCTO))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOPRODUCTO))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.CNOMBREPRODUCTO))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.CDESCRIPCIONPRODUCTO))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.CPRECIO1))           // Lista 1 = Menudeo
            .ForMember(dest => dest.Precio2, opt => opt.MapFrom(src => src.CPRECIO2))          // Lista 2 = Mayoreo
            .ForMember(dest => dest.Impuesto1, opt => opt.MapFrom(src => src.CIMPUESTO1))      // % IVA (e.g. 16.0)
            .ForMember(dest => dest.CodigoSat, opt => opt.MapFrom(src => src.CCLAVESAT))
            .ForMember(dest => dest.UnidadMedidaId, opt => opt.MapFrom(src => src.CIDUNIDADBASE))
            .ForMember(dest => dest.UnidadMedidaNombre, opt => opt.MapFrom(src => src.UnidadMedidaBase != null ? src.UnidadMedidaBase.NombreUnidad : string.Empty))
            .ForMember(dest => dest.Clasificacion1Id, opt => opt.MapFrom(src => src.CIDVALORCLASIFICACION1))
            .ForMember(dest => dest.Clasificacion5Id, opt => opt.MapFrom(src => src.CIDVALORCLASIFICACION5))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.CSTATUSPRODUCTO == 1));

        CreateMap<AdmClientes, ClienteDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDCLIENTEPROVEEDOR))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOCLIENTE))
            .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.CRAZONSOCIAL))
            .ForMember(dest => dest.RFC, opt => opt.MapFrom(src => src.CRFC))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CEMAIL1))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.CESTATUS == 1));

        CreateMap<AdmAlmacenes, AlmacenDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDALMACEN))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOALMACEN))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.CNOMBREALMACEN));

        CreateMap<AdmAgentes, AgenteDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CodigoAgente))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreAgente))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.TipoAgente));

        CreateMap<AdmConceptos, ConceptoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CIDCONCEPTODOCUMENTO))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.CCODIGOCONCEPTO))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.CNOMBRECONCEPTO))
            .ForMember(dest => dest.Serie, opt => opt.MapFrom(src => src.CSERIEPOROMISION));

        CreateMap<AdmMonedas, MonedaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreMoneda))
            .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Simbolo));

        CreateMap<AdmUnidadesMedidaPeso, UnidadMedidaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreUnidad))
            .ForMember(dest => dest.Abreviatura, opt => opt.MapFrom(src => src.Abreviatura));
    }
}