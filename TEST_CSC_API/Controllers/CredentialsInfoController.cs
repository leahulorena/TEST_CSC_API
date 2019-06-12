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

        [HttpGet]
        public string CredentialsInfo(string id)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsInfoClient credentialsInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);
            object response = credentialsInfoClient.GetCredentialsInfo("5dea55c2-13bc-429a-bf5b-2127740395c1", "863971CBC7BF63D49C9F14809FD5A1142B75E9AB");

            return serializer.Serialize(response);
        }


    }

    public class CredentialsInfoClient : BaseClient
    {
        public CredentialsInfoClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsInfo(string access_token, string id)
        {
            InputCredentialsInfo credentialsInfo = new InputCredentialsInfo()
            {
                credentialID = id,
                certInfo = true
            };

            RestRequest request = new RestRequest("credentials/info", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            //var postData = serializer.Serialize(credentialsInfo);
            var postData = "{ \"credentialID\": \"" + id + "\",\"certInfo\": \"true\"}";
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;
        }
    }
}