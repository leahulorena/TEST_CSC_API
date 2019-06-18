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
    public class CredentialsInfoController : ControllerBase
    {
        IConfiguration _configuration;

        public CredentialsInfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public string CredentialsInfo(InputCredentialsInfo inputCredentialsInfo)
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

            CredentialsInfoClient credentialsInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);
            object response = credentialsInfoClient.GetCredentialsInfo(access_token, inputCredentialsInfo);

            return serializer.Serialize(response);
        }


    }

    public class CredentialsInfoClient : BaseClient
    {
        public CredentialsInfoClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsInfo(string access_token, InputCredentialsInfo inputCredentialsInfo)
        {
            
            RestRequest request = new RestRequest("credentials/info", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsInfo);
           // var postData = "{ \"credentialID\": \"" + credentialsID + "\",\"certInfo\": \"true\"}";
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;
        }
    }
}