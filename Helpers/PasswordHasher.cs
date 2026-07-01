using System.Security.Cryptography;

namespace HealthcareCRM.Helpers
{
    /// <summary>
    /// Provides PBKDF2-based password hashing and verification using SHA-256.
    /// </summary>
    public class PasswordHasher
    {
        private const int SALT_SIZE  = 16;  // bytes
        private const int KEY_SIZE   = 32;  // bytes
        private const int ITERATIONS = 100_000;

        /// <summary>
        /// Hashes a plain-text password using PBKDF2-SHA256 with a random salt.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>Base64-encoded string of salt + hash bytes.</returns>
        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                ITERATIONS,
                HashAlgorithmName.SHA256,
                KEY_SIZE);

            var combined = new byte[SALT_SIZE + KEY_SIZE];
            Buffer.BlockCopy(salt, 0, combined, 0,         SALT_SIZE);
            Buffer.BlockCopy(hash, 0, combined, SALT_SIZE, KEY_SIZE);
            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Verifies a plain-text password against a stored PBKDF2 hash.
        /// </summary>
        /// <param name="hashedPassword">Stored Base64-encoded salt + hash.</param>
        /// <param name="password">The plain-text password to verify.</param>
        /// <returns><c>true</c> if the password matches; <c>false</c> otherwise.</returns>
        public bool VerifyPassword(string hashedPassword, string password)
        {
            var combined = Convert.FromBase64String(hashedPassword);
            var salt     = combined[..SALT_SIZE];
            var storedHash = combined[SALT_SIZE..];

            var derivedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                ITERATIONS,
                HashAlgorithmName.SHA256,
                KEY_SIZE);

            return CryptographicOperations.FixedTimeEquals(derivedHash, storedHash);
        }
    }
}
