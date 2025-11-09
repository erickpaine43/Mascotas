// AutoMapperProfiles/AutoMapperProfile.cs
using AutoMapper;
using Mascotas.Dto;
using Mascotas.Models;
using Newtonsoft.Json;

namespace Mascotas.AutoMapperProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ========== USUARIOS ==========
            CreateMap<RegisterDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<CreateUsuarioDto, Usuario>();
            CreateMap<UpdateUsuarioDto, Usuario>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ========== CLIENTES ==========
            CreateMap<Cliente, ClienteDto>();

            // ========== CATEGORÍAS ==========
            CreateMap<Categoria, CategoriaDto>()
                .ForMember(dest => dest.TotalProductos, opt => opt.MapFrom(src => src.Productos.Count(p => p.Activo)));
            CreateMap<CreateCategoriaDto, Categoria>();
            CreateMap<UpdateCategoriaDto, Categoria>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ========== PRODUCTOS ==========
            // ✅ CORREGIDO: Solo UN mapeo de Producto a ProductoDto
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre,
                    opt => opt.MapFrom(src => src.Categoria.Nombre))
                .ForMember(dest => dest.ImagenesAdicionales,
                    opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ImagenesAdicionales)
                        ? new List<string>()
                        : JsonConvert.DeserializeObject<List<string>>(src.ImagenesAdicionales)));

            CreateMap<CreateProductoDto, Producto>()
                .ForMember(dest => dest.ImagenesAdicionales,
                    opt => opt.MapFrom(src => src.ImagenesAdicionales != null && src.ImagenesAdicionales.Any()
                        ? JsonConvert.SerializeObject(src.ImagenesAdicionales)
                        : null));

            CreateMap<UpdateProductoDto, Producto>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ========== ANIMALES/MASCOTAS ==========
            CreateMap<Animal, AnimalDto>();
            CreateMap<CreateAnimalDto, Animal>();
            CreateMap<UpdateAnimalDto, Animal>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ========== CARRITO ==========
            CreateMap<Carrito, CarritoDto>();
            CreateMap<CarritoItem, CarritoItemDto>()
                .ForMember(dest => dest.Mascota, opt => opt.MapFrom(src => src.Mascota))
                .ForMember(dest => dest.Producto, opt => opt.MapFrom(src => src.Producto));

            // ========== ÓRDENES ==========
            CreateMap<Orden, OrdenDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => src.MetodoPago.ToString()))
                .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente));

            CreateMap<OrdenItem, OrdenItemDto>()
                .ForMember(dest => dest.Animal, opt => opt.MapFrom(src => src.Animal))
                .ForMember(dest => dest.Producto, opt => opt.MapFrom(src => src.Producto));

            CreateMap<CreateOrdenDto, Orden>();
            CreateMap<CreateOrdenItemDto, OrdenItem>();

            // ========== DTOs PARA CREACIÓN ==========
            CreateMap<AgregarAlCarritoDto, CarritoItem>();

            CreateMap<CreateReviewRequest, Review>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore());

            // UpdateReviewRequest -> Review
            CreateMap<UpdateReviewRequest, Review>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ReviewId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductoId, opt => opt.Ignore())
                .ForMember(dest => dest.ClienteId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore());

            // Review -> ReviewResponse
            CreateMap<Review, ReviewResponse>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src =>
                    src.Cliente != null ? src.Cliente.Nombre : "Cliente no disponible"))
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src =>
                    src.Producto != null ? src.Producto.Nombre : "Producto no disponible"));
        }
    }
}