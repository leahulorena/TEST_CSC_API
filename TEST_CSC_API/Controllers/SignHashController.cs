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
    public class SignHashController : ControllerBase
    {
        IConfiguration _configuration;

        public SignHashController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public string SignHash(InputSignaturesSignHash inputSignatures)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;


            Microsoft.Extensions.Primitives.StringValues value;
            string access_token = "";
            if (Request.Headers.TryGetValue("Authorization", out value))
            {
                access_token = value.ToString().Replace("Bearer ", "");
            }
            else
            {
                OutputError error = new OutputError()
                {
                    error = "invalid_access_token",
                    error_description = "Invalid access_token"
                };
                return serializer.Serialize(error);
            }


            SignHashClient signHashClient = new SignHashClient(serializer, errorLogger, baseURL);
            object response = signHashClient.GetSignedHash(access_token, inputSignatures);

            return serializer.Serialize(response);
        }
    }


    public class SignHashClient : BaseClient
    {
        public SignHashClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetSignedHash(string access_token, InputSignaturesSignHash inputSignatures)
        {       
            
            RestRequest request = new RestRequest("signatures/signHash", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputSignatures);            
            request.AddJsonBody(postData);
            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);
            return data;

        }
    }
}