
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Base.Helper
{
    public static class AuthUtils
    {

        /// <summary>
        /// Überprüft, ob das übergebene Passwort unter Verwendung des Salt
        /// nach dem Hashen mit dem gespeicherten Passwort übereinstimmt.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedSaltetPassword"></param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string hashedSaltetPassword)
        {
            var saltHex = hashedSaltetPassword.Substring(hashedSaltetPassword.Length - 32, 32);
            var salt = ByteArrayHelper.HexStringToByteArray(saltHex);
            string hashText = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashText + saltHex == hashedSaltetPassword;
        }

        /// <summary>
        /// Erzeugt zufälligen Salt, hased das Passwort mit dem Salt, fügt
        /// den Salt hinten an und liefert das Ergebnis zurück
        /// </summary>
        /// <param name="password">Passwort im Klartext</param>
        /// <returns>gesaltetes und gehashtes Passwort</returns>
        public static string GenerateHashedPassword(string password)
        {
            // https://docs.microsoft.com/de-de/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-3.1
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashText = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashText + ByteArrayHelper.ByteArrayToHexString(salt);
        }

        public static SigningCredentials GetSigningCredentials()
        {
            var secretKey = ConfigurationHelper.GetConfiguration("SecretKey","APISettings");
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public static async Task<List<Claim>> GetClaims(IdentityUser user, UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Email),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id),

            };
            var roles = await userManager.GetRolesAsync(await userManager.FindByEmailAsync(user.Email));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        public static bool RoleChanged(string newRole, IList<string> oldRoles)
        {
            if (oldRoles.Count == 0)
            {
                return true;
            }
            return newRole != oldRoles[0];
        }

        /// <summary>
        /// Id des eingeloggten Benutzers zurückgeben
        /// Id ist als Claim gespeichert
        /// </summary>
        /// <param name="principal"></param>
        /// <returns>Id oder null</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetLoggedInUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var id = principal.Claims.FirstOrDefault(x => x.Type == "Id");
            return id.Value;
        }






    }
}
