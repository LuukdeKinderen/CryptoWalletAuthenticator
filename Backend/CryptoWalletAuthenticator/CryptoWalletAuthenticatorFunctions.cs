using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Nethereum.Signer;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CryptoWalletAuthenticator
{
    public class CryptoWalletAuthenticatorFunctions
    {
        private readonly IConfiguration _configuration;
        private readonly UserContext _userContext;
        private readonly EthereumMessageSigner _signer;

        public CryptoWalletAuthenticatorFunctions(UserContext userContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _userContext = userContext;
            _signer = new EthereumMessageSigner();
        }

        [FunctionName("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            IEnumerable<object> users =  _userContext.Users.Select(u => new
            {
                walletAddress = u.WalletAdress,
                favoriteColour = u.FavoriteColour
            }).ToList();

            return new OkObjectResult(users);
        }


        [FunctionName("ReadNonce")]
        public async Task<IActionResult> ReadNonce(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // get parameter
            string walledAdress = req.Query["walletAdress"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            walledAdress = walledAdress ?? data?.walletAdress;

            if (String.IsNullOrEmpty(walledAdress))
            {
                return new BadRequestObjectResult("walledAdress can not be empty");
            }

            //get user from database
            User u = _userContext.Users.Where(u => u.WalletAdress == walledAdress).FirstOrDefault();

            //create user if not exists
            if (u == null)
            {
                u = new User()
                {
                    WalletAdress = walledAdress,
                    Nonce = GetNewRandomNonce()
                };
                _userContext.Users.Add(u);
                _userContext.SaveChanges();
                log.LogInformation($"New user created: {u.WalletAdress}");
            }


            //get nonce for that user
            string responseMessage = u.Nonce;

            log.LogInformation($"User:{u.WalletAdress} can now sign Nonce:{responseMessage}");

            return new OkObjectResult(responseMessage);
        }


        [FunctionName("ValidateSignedNonce")]
        public async Task<IActionResult> ValidateSignedNonce(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //get parameters
            string signedNonce = req.Query["signedNonce"];
            string nonce = req.Query["nonce"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            signedNonce = signedNonce ?? data?.signedNonce;
            nonce = nonce ?? data?.nonce;

            if (String.IsNullOrEmpty(signedNonce))
            {
                return new BadRequestObjectResult("signedNonce can not be empty");
            }
            if (String.IsNullOrEmpty(nonce))
            {
                return new BadRequestObjectResult("nonce can not be empty");
            }

            //find the address by which the nonce is signed 
            string walletAdress = _signer.EncodeUTF8AndEcRecover(nonce, signedNonce);
            walletAdress = walletAdress.ToLower();

            //get user with that adress
            User u = _userContext.Users.Where(u => u.WalletAdress == walletAdress).SingleOrDefault();

            //throw error if user does not exist
            if (u == null)
            {
                string errorMessage = $"Authentication was not succesfull! User with wallet adress:{walletAdress} not found";
                log.LogError(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }

            //throw error if nonce is incorrect
            if (u.Nonce != nonce)
            {
                string errorMessage = "Authentication was not succesfull! Nonce was incorrect";
                log.LogError(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }

            //authenticate user correctly
            string succesMessage = $"User: {walletAdress} is succesfully authenticated!";
            log.LogInformation(succesMessage);

            //Refresh nonce
            u.Nonce = GetNewRandomNonce();
            _userContext.Users.Update(u);
            _userContext.SaveChanges();

            //generate jwt and return to user
            GenerateJWTToken generateJWTToken = new GenerateJWTToken(_configuration);
            string token = generateJWTToken.IssuingJWT(walletAdress);
            return new OkObjectResult(token);
        }

        [FunctionName("UpdateFavoriteColour")]
        public async Task<IActionResult> UpdateFavoriteColour(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //User JWT authentication
            ValidateJWT auth = new ValidateJWT(req, _configuration);
            if (!auth.IsValid)
            {
                string errorMessage = "Unauthorized, log in first. Or try logging in and out";
                log.LogError(errorMessage);

                ObjectResult objectResult = new ObjectResult(errorMessage);
                objectResult.StatusCode = 401; // Unauthorized code
                return objectResult;
            }

            //get parameter
            string colour = req.Query["colour"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            colour = colour ?? data?.colour;

            if (String.IsNullOrEmpty(colour))
            {
                string errorMessage = "colour can not be empty";
                log.LogError(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }

            //get user Using auth wallet adress
            User u = _userContext.Users.Where(u => u.WalletAdress == auth.WalletAdress).SingleOrDefault();

            //throw error if user does not exist
            if (u == null)
            {
                string errorMessage = $"Authentication was not succesfull! User with wallet adress:{auth.WalletAdress} not found";
                log.LogError(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }

            //set colour and update database
            u.FavoriteColour = colour;
            _userContext.Users.Update(u);
            _userContext.SaveChanges();

            //renerate responce
            string returnMessage = $"User:{auth.WalletAdress} was authenticated, favorite colour was changed to:{colour}";
            log.LogInformation(returnMessage);
            return new OkObjectResult(returnMessage);
        }

        private string GetNewRandomNonce()
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

        }
    }
}
