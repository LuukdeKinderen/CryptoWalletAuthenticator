using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWalletAuthenticator
{
    internal class ValidateJWT
    {
        public bool IsValid
        {
            get;
        }
        public string WalletAdress
        {
            get;
        }

        public ValidateJWT(HttpRequest req, IConfiguration configuration)
        {
            if (!req.Headers.ContainsKey("Authorization"))
            {
                IsValid = false;
                return;
            }

            string authorizationHeader = req.Headers["Authorization"];

            if (String.IsNullOrEmpty(authorizationHeader))
            {
                IsValid = false;
                return;
            }

            IDictionary<string, object> claims = null;
            try
            {
                if (authorizationHeader.StartsWith("Bearer"))
                {
                    authorizationHeader = authorizationHeader.Substring(7);
                }
                claims = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(configuration["JwtKey"])
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(authorizationHeader);
            }
            catch (Exception ex)
            {
                IsValid = false;
                return;
            }

            if (!claims.ContainsKey("walletAdress"))
            {
                IsValid = false;
                return;
            }
            IsValid = true;
            WalletAdress = Convert.ToString(claims["walletAdress"]);
        }
    }
}
