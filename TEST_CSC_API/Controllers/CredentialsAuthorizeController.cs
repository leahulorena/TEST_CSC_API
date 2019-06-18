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

        [HttpPost]
        public string GetCredentialsAuthorize(InputCredentialsAuthorize inputCredentialsAuthorize)
        {

            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsAuthorizeClient credentialsAuthorizeClient = new CredentialsAuthorizeClient(serializer, errorLogger, baseURL);

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

            object response = credentialsAuthorizeClient.GetCredentialsAuthorize(access_token, inputCredentialsAuthorize);
            return serializer.Serialize(response);
        }

    }

    public class CredentialsAuthorizeClient : BaseClient
    {
        public CredentialsAuthorizeClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsAuthorize(string access_token, InputCredentialsAuthorize inputCredentialsAuthorize)
        {
            
            
            RestRequest request = new RestRequest("credentials/authorize", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsAuthorize);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;

        }
    }


}