using System.Net.Mail;

namespace ProjectMmApi.Utilities
{
    public static class Validator
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var emailAddress = new MailAddress(email);
                return emailAddress != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStrongPassword(string password)
        {
            bool hasMinimumLength = password.Length >= 8;
            bool hasUppercase = password.Any(c => char.IsUpper(c));
            bool hasLowercase = password.Any(c => char.IsLower(c));
            bool hasWhiteSpace = password.Any(c => char.IsWhiteSpace(c));

            return hasMinimumLength && hasUppercase && hasLowercase && (!hasWhiteSpace);
        }
    }
}
