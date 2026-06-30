using System.Security.Cryptography;
using System.Text;

namespace HealthcareCRM.Helpers
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
            var key = algorithm.GetBytes(KeySize);
            var salt = algorithm.Salt;
            return Convert.ToBase64String(Combine(salt, key));
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            var value = Convert.FromBase64String(hashedPassword);
            var salt = value[..SaltSize];
            var key = value[SaltSize..];

            using var algorithm = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var keyToCheck = algorithm.GetBytes(KeySize);
            return keyToCheck.SequenceEqual(key);
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            var ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
