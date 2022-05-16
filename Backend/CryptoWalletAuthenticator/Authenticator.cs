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

namespace CryptoWalletAuthenticator
{
    public class Authenticator
    {
        private readonly UserContext _userContext;
        private readonly EthereumMessageSigner _signer;

        public Authenticator(UserContext userContext)
        {
            _userContext = userContext;
            _signer = new EthereumMessageSigner();
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
                throw new Exception(errorMessage);
            }

            //throw error if nonce is incorrect
            if (u.Nonce != nonce)
            {
                string errorMessage = "Authentication was not succesfull! Nonce was incorrect";
                log.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            //authenticate user correctly
            string succesMessage = $"User: {walletAdress} is succesfully authenticated!";
            log.LogInformation(succesMessage);

            return new OkObjectResult(succesMessage);
        }

        private string GetNewRandomNonce()
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

        }
    }
}
