using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ArabamTR.Data;
using ArabamTR.Models;
using ArabamTR.Models.Dtos;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ArabamTRDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
                return BadRequest("Bu e-posta adresiyle zaten bir kullanıcı kayıtlı.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                IsEmailConfirmed = true, // Auto confirmed for mock system
                Is2FAEnabled = false,
                AccountStatus = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt işlemi başarıyla tamamlandı." });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Geçersiz e-posta veya şifre.");

            if (user.AccountStatus != "Active")
                return Forbid("Hesabınız aktif değil.");

            if (user.Is2FAEnabled)
            {
                // Generate 6-digit 2FA code
                var random = new Random();
                var twoFactorCode = random.Next(100000, 999999).ToString();
                
                user.TwoFactorCode = twoFactorCode;
                await _context.SaveChangesAsync();

                // In production, you would send this code via email/SMS. 
                // We return it here as 'codeForTesting' to make testing the API easier.
                return Ok(new 
                { 
                    status = "2FA_GEREKLI", 
                    message = "İki aşamalı doğrulama kodu gönderildi.",
                    codeForTesting = twoFactorCode 
                });
            }

            var token = GenerateJwtToken(user);
            return Ok(new 
            { 
                status = "OK", 
                token = token,
                user = new { id = user.Id, name = user.Name, email = user.Email }
            });
        }

        // POST: api/auth/verify-2fa
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            if (string.IsNullOrEmpty(user.TwoFactorCode) || user.TwoFactorCode != dto.Code)
                return BadRequest("Doğrulama kodu hatalı veya süresi geçmiş.");

            // Clear the 2FA code after successful verification
            user.TwoFactorCode = null;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return Ok(new 
            { 
                status = "OK", 
                token = token,
                user = new { id = user.Id, name = user.Name, email = user.Email }
            });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("Bu e-posta adresine kayıtlı bir kullanıcı bulunamadı.");

            // Generate a password reset token (GUID)
            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            await _context.SaveChangesAsync();

            // In production, send email. We return it here as 'tokenForTesting' for ease of testing.
            return Ok(new 
            { 
                message = "Şifre sıfırlama kodu oluşturuldu.",
                tokenForTesting = resetToken 
            });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.ResetPasswordToken == dto.Token);
            if (user == null)
                return BadRequest("Geçersiz e-posta adresi veya sıfırlama kodu.");

            // Hash the new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordHash = hashedPassword;
            user.ResetPasswordToken = null; // Clear token

            await _context.SaveChangesAsync();

            return Ok(new { message = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz." });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["Jwt:Secret"] ?? "ArabamTRSuperSecretKeyWhichIsAtLeast32BytesLong!";
            var key = Encoding.UTF8.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"] ?? "ArabamTR",
                Audience = _configuration["Jwt:Audience"] ?? "ArabamTR",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
