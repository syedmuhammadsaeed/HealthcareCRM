using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthcareCRM.Helpers;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using HealthcareCRM.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthcareCRM.Services
{
    /// <summary>
    /// Handles user registration, login, and JWT token generation.
    /// </summary>
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings     _jwtSettings;
        private readonly PasswordHasher  _passwordHasher;

        /// <summary>
        /// Initialises AuthService with required dependencies.
        /// </summary>
        public AuthService(
            IUserRepository          userRepository,
            IOptions<JwtSettings>    jwtOptions,
            PasswordHasher           passwordHasher)
        {
            _userRepository = userRepository;
            _jwtSettings    = jwtOptions.Value;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Registers a new user. Returns an error if the email is already taken.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var existingUser = await _userRepository.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return (false, "Email address is already registered.");
            }

            var user = new User
            {
                Name         = model.FullName,
                Email        = model.Email,
                PasswordHash = _passwordHasher.HashPassword(model.Password),
                CreatedDate  = DateTime.UtcNow,
                Role         = model.Role,
                Status       = "Pending", // Require Super Admin approval
                Specialization = model.Role == "Doctor" ? model.Specialization : null,
                Phone          = model.Role == "Doctor" ? model.Phone : null,
                Address        = model.Role == "Doctor" ? model.Address : null
            };

            await _userRepository.AddAsync(user);
            return (true, "Registration completed successfully.");
        }

        /// <summary>
        /// Validates credentials and returns a signed JWT token on success.
        /// </summary>
        public async Task<(bool IsSuccess, string Token, string Message)> LoginAsync(LoginViewModel model)
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, string.Empty, "Invalid email or password.");
            }

            if (user.Role != model.Role)
            {
                return (false, string.Empty, "Invalid role for this user.");
            }

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, model.Password))
            {
                return (false, string.Empty, "Invalid email or password.");
            }

            if (user.Role != "SuperAdmin")
            {
                if (user.Status == "Pending")
                {
                    return (false, string.Empty, "Your account is pending approval by the Super Admin.");
                }
                if (user.Status == "Rejected")
                {
                    return (false, string.Empty, "Your account registration was rejected.");
                }
            }

            var token = generateJwtToken(user);
            return (true, token, "Login successful.");
        }

        public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, model.CurrentPassword))
            {
                return (false, "Incorrect current password.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(model.NewPassword);
            await _userRepository.UpdateAsync(user);

            return (true, "Password updated successfully.");
        }

        // ── Private Helpers ───────────────────────────────────────────────

        private string generateJwtToken(User user)
        {
            var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             _jwtSettings.Issuer,
                audience:           _jwtSettings.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task SeedSuperAdminAsync()
        {
            var adminEmail = "superadmin@healthcarecrm.com";
            var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new User
                {
                    Name = "Super Admin",
                    Email = adminEmail,
                    PasswordHash = _passwordHasher.HashPassword("SuperAdmin123!"),
                    CreatedDate = DateTime.UtcNow,
                    Role = "SuperAdmin",
                    Status = "Approved"
                };
                await _userRepository.AddAsync(admin);
            }
        }
    }
}
