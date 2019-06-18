using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using RestSharp.Deserializers;
using Microsoft.Extensions.Configuration;


namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredentialsSendOTPController : ControllerBase
    {
        IConfiguration _configuration;

        public CredentialsSendOTPController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public string SendOTP(InputCredentialsSendOTP inputCredentialsSendOTP)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsSendOTPClient sendOTPClient = new CredentialsSendOTPClient(serializer, errorLogger, baseURL);

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


            object response = sendOTPClient.GetCredentialsSendOTP(access_token, inputCredentialsSendOTP);

            return serializer.Serialize(response);
        }
    }

    public class CredentialsSendOTPClient : BaseClient
    {
        public CredentialsSendOTPClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsSendOTP (string access_token, InputCredentialsSendOTP inputCredentialsSendOTP)
        {
           
            RestRequest request = new RestRequest("credentials/sendOTP", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsSendOTP);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);
            return data;

        }
    }
}