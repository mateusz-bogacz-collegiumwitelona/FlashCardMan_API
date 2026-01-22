using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Data.Helpers
{
    public abstract class TokenCreator
    {
        protected string GenerateToken()
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(16);

            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
