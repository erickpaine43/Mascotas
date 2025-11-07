// AutoMapperProfiles/Automapper.cs
using AutoMapper;
using Mascotas.Dto;
using Mascotas.Models;
using Newtonsoft.Json;


namespace Mascota.AutoMapperProfiles
{
    public class Automapper : Profile
    {
        public Automapper()
        {
            // Mapeos existentes
            CreateMap<RegisterDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<CreateUsuarioDto, Usuario>();
            CreateMap<UpdateUsuarioDto, Usuario>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            
            CreateMap<Categoria, CategoriaDto>()
                .ForMember(dest => dest.TotalProductos, opt => opt.MapFrom(src => src.Productos.Count(p => p.Activo)));
            CreateMap<CreateCategoriaDto, Categoria>();
            CreateMap<UpdateCategoriaDto, Categoria>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

           
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria.Nombre));

            CreateMap<Producto, ProductoDto>()
           .ForMember(dest => dest.CategoriaNombre,
                      opt => opt.MapFrom(src => src.Categoria.Nombre))
           .ForMember(dest => dest.ImagenesAdicionales,
                      opt => opt.MapFrom(src =>
                          string.IsNullOrEmpty(src.ImagenesAdicionales)
                              ? new List<string>()
                              : JsonConvert.DeserializeObject<List<string>>(src.ImagenesAdicionales)));
        }
    }
}