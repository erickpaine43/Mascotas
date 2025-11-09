using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Mascotas.AutoMapperProfiles;
using AutoMapper;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(MascotaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/reviews/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetProductReviews(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductoId == productId && r.Activo)
                    .Include(r => r.Cliente)
                    .ThenInclude(c => c.Usuario)
                    .Include(r => r.Producto)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToListAsync();

                var response = _mapper.Map<List<ReviewResponse>>(reviews);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/reviews/product/5/average-rating
        [HttpGet("product/{productId}/average-rating")]
        public async Task<ActionResult<AverageRatingResponse>> GetAverageRating(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductoId == productId && r.Activo)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return Ok(new AverageRatingResponse
                    {
                        AverageRating = 0,
                        TotalReviews = 0
                    });
                }

                var average = reviews.Average(r => r.Rating);
                var total = reviews.Count;

                return Ok(new AverageRatingResponse
                {
                    AverageRating = Math.Round(average, 1),
                    TotalReviews = total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/reviews/product/5/rating-distribution
        [HttpGet("product/{productId}/rating-distribution")]
        public async Task<ActionResult<RatingDistributionResponse>> GetRatingDistribution(int productId)
        {
            try
            {
                var distribution = await _context.Reviews
                    .Where(r => r.ProductoId == productId && r.Activo)
                    .GroupBy(r => r.Rating)
                    .Select(g => new RatingCount
                    {
                        Rating = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.Rating)
                    .ToListAsync();

                var total = distribution.Sum(d => d.Count);

                return Ok(new RatingDistributionResponse
                {
                    Distribution = distribution,
                    TotalReviews = total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ReviewResponse>> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                // Validar que el cliente existe
                var cliente = await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == request.ClienteId);

                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                // Validar que el producto existe
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                if (producto == null)
                    return NotFound("Producto no encontrado");

                // Verificar si el cliente ya hizo una reseña
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductoId == request.ProductId &&
                                             r.ClienteId == request.ClienteId &&
                                             r.Activo);

                if (existingReview != null)
                    return BadRequest("Ya has realizado una reseña para este producto");

                // Usar AutoMapper para crear la review
                var review = _mapper.Map<Review>(request);

                // Validar el modelo
                var validationContext = new ValidationContext(review);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(review, validationContext, validationResults, true))
                {
                    return BadRequest(validationResults.First().ErrorMessage);
                }

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Mapear a Response
                var response = _mapper.Map<ReviewResponse>(review);
                response.ClienteNombre = cliente.Nombre; // Asignar nombre del cliente

                return CreatedAtAction(nameof(GetProductReviews), new { productId = review.ProductoId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: api/reviews/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ReviewResponse>> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.Cliente)
                    .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(r => r.ReviewId == id && r.Activo);

                if (review == null)
                    return NotFound("Reseña no encontrada");

                // Usar AutoMapper para actualizar
                _mapper.Map(request, review);

                // Validar el modelo
                var validationContext = new ValidationContext(review);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(review, validationContext, validationResults, true))
                {
                    return BadRequest(validationResults.First().ErrorMessage);
                }

                await _context.SaveChangesAsync();

                var response = _mapper.Map<ReviewResponse>(review);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // DELETE: api/reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound("Reseña no encontrada");
                }

                // Soft delete
                review.Activo = false;
                review.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

       
        // GET: api/reviews/client/5
        [HttpGet("client/{clienteId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetClientReviews(int clienteId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ClienteId == clienteId && r.Activo)
                    .Include(r => r.Cliente)
                    .Include(r => r.Producto)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToListAsync();

                var response = _mapper.Map<List<ReviewResponse>>(reviews);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
    }
