
using Base.DataTransferObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Base.Helper
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public static string[] ParseRolesFromJwt(string jwt)
        {
            var claims = ParseClaimsFromJwt(jwt);
            var roles = claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(x => x.Value)
                .ToArray();
            return roles;
        }

        public static string ParseMailFromJwt(string jwt)
        {
            var claims = ParseClaimsFromJwt(jwt);
            var mail = claims
                .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                .Select(x => x.Value)
                .FirstOrDefault();
            return mail;
        }

        public static string ParseIdFromJwt(string jwt)
        {
            var claims = ParseClaimsFromJwt(jwt);
            var id = claims
                .Where(c => c.Type == "Id")
                .Select(x => x.Value)
                .FirstOrDefault();
            return id;
        }

        public static DateTime? ParseExpirationTimeFromJwt(string jwt)
        {
            var claims = ParseClaimsFromJwt(jwt);
            var exp = claims
                .Where(c => c.Type == "exp")
                .Select(x => x.Value)
                .FirstOrDefault();
            if (exp == null)
            {
                return null;
            }
            long unixTimeStamp = long.Parse(exp);
            var time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            time = time.AddSeconds(unixTimeStamp).ToLocalTime();
            return time;
        }

        public static UserDetailsDto ParseUserDetails(string jwt)
        {
            var userDetails = new UserDetailsDto
            {
                Email = ParseMailFromJwt(jwt),
                RoleName = ParseRolesFromJwt(jwt).FirstOrDefault(),
                Id = ParseIdFromJwt(jwt)
            };
            return userDetails;
        }


    }
}
