using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace CryptoWalletAuthenticator
{
    internal class GenerateJWTToken
    {
        private readonly IConfiguration _configuration;

        private readonly IJwtAlgorithm _algorithm;
        private readonly IJsonSerializer _serializer;
        private readonly IBase64UrlEncoder _base64Encoder;
        private readonly IJwtEncoder _jwtEncoder;
        public GenerateJWTToken(IConfiguration configuration)
        {
            _configuration = configuration;

            _algorithm = new HMACSHA256Algorithm();
            _serializer = new JsonNetSerializer();
            _base64Encoder = new JwtBase64UrlEncoder();
            _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);
        }
        public string IssuingJWT(string user)
        {
            Dictionary<string, object> claims = new Dictionary<string, object> {
                {
                    "walletAdress",
                    user
                },
                {
                    "expiryDate",
                    DateTime.Now + TimeSpan.FromMinutes(30)
                }
            };
            string token = _jwtEncoder.Encode(claims, _configuration["JwtKey"]); 
            return token;
        }
    }
}
