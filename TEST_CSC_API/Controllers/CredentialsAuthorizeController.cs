using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Deserializers;

namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredentialsAuthorizeController : ControllerBase
    {
        IConfiguration _configuration;

        public CredentialsAuthorizeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public string GetCredentialsAuthorize()
        {

            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsAuthorizeClient credentialsAuthorizeClient = new CredentialsAuthorizeClient(serializer, errorLogger, baseURL);

            object response = credentialsAuthorizeClient.GetCredentialsAuthorize("863971CBC7BF63D49C9F14809FD5A1142B75E9AB", 1, "9508861a-4a19-4e8a-8f4a-6cd78c861366", "ysijd6", "L0r3n@L3@hu***");
            return serializer.Serialize(response);
        }

    }

    public class CredentialsAuthorizeClient : BaseClient
    {
        public CredentialsAuthorizeClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsAuthorize(string id, int numSignatures, string access_token, string otp, string pin)
        {
            byte[] bytes = Encoding.GetBytes("text to hash");
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashBase64 = Convert.ToBase64String(hash);
            string[] hashToSign = new string[] { hashBase64 };

            InputCredentialsAuthorize credentialsAuthorize = new InputCredentialsAuthorize()
            {
                clientData = "",
                credentialID = id,
                description = "",
                numSignatures = numSignatures,
                OTP = otp,
                PIN = pin,
                hash = hashToSign,
            };

            RestRequest request = new RestRequest("credentials/authorize", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(credentialsAuthorize);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;

        }
    }


}