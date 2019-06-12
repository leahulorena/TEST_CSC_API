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

        [HttpGet]
        public string SendOTP(string id)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsSendOTPClient sendOTPClient = new CredentialsSendOTPClient(serializer, errorLogger, baseURL);
            object response = sendOTPClient.GetCredentialsSendOTP("9508861a-4a19-4e8a-8f4a-6cd78c861366", "863971CBC7BF63D49C9F14809FD5A1142B75E9AB");

            return serializer.Serialize(response);
        }
    }

    public class CredentialsSendOTPClient : BaseClient
    {
        public CredentialsSendOTPClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsSendOTP (string access_token, string credentialID)
        {
            InputCredentialsSendOTP sendOTP = new InputCredentialsSendOTP()
            {
                clientData = "",
                credentialID = credentialID
            };

            RestRequest request = new RestRequest("credentials/sendOTP", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(sendOTP);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);
            return data;

        }
    }
}