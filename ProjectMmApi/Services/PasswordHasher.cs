using System.Security.Cryptography;
using ProjectMmApi.Interfaces;

namespace ProjectMmApi.Services
{
    public class PasswordHasher:IPasswordHasher
    {
        private const int SaltSize = 128 / 8; // 16 bytes
        private const int KeySize = 256 / 8; // 32 bytes
        private const int Iterations = 10_000;
        private const char Delimeter = ';';
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            return String.Join(Delimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            var elements = passwordHash.Split(Delimeter);

            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var inputPasswordHash = Rfc2898DeriveBytes.Pbkdf2(inputPassword, salt, Iterations, _hashAlgorithmName, KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, inputPasswordHash);
        }
    }
}
