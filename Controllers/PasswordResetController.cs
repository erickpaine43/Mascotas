using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mascotas.Dto;
using Mascotas.Services;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<PasswordResetController> _logger;

        public PasswordResetController(
            IPasswordResetService passwordResetService,
            ILogger<PasswordResetController> logger)
        {
            _passwordResetService = passwordResetService;
            _logger = logger;
        }

        [HttpPost("solicitar-reset")]
        public async Task<IActionResult> SolicitarResetPassword([FromBody] PasswordResetRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensaje = "Email inválido" });
            }

            var resultado = await _passwordResetService.SolicitarResetPasswordAsync(requestDto.Email);

            // Siempre devolver el mismo mensaje por seguridad
            return Ok(new
            {
                mensaje = "Si el email existe en nuestro sistema, recibirás un código para restablecer tu contraseña."
            });
        }

        [HttpPost("resetear-con-codigo")]
        public async Task<IActionResult> ResetearConCodigo([FromBody] ResetPasswordWithCodeDto resetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            if (resetDto.NuevaPassword != resetDto.ConfirmarPassword)
            {
                return BadRequest(new { mensaje = "Las contraseñas no coinciden" });
            }

            var resultado = await _passwordResetService.ResetearPasswordConCodigoAsync(
                resetDto.Email,
                resetDto.Codigo,
                resetDto.NuevaPassword
            );

            if (!resultado)
            {
                return BadRequest(new { mensaje = "Código inválido, expirado o email incorrecto" });
            }

            return Ok(new { mensaje = "Contraseña restablecida exitosamente" });
        }

        
    }
}