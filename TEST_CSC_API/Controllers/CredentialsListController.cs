using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp.Deserializers;
using RestSharp;
using Microsoft.Extensions.Configuration;

namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredentialsListController : ControllerBase
    {
        IConfiguration _configuration;


        public CredentialsListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public string CredentialsList()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsListClient credentialsListClient = new CredentialsListClient(serializer, errorLogger, baseURL);
            object response = credentialsListClient.GetCredentialsList("6445f209-d5fd-4fa1-aceb-2e1f556f2840");
            return serializer.Serialize(response);
        }

    }

    public class CredentialsListClient : BaseClient
    {
        public CredentialsListClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
           base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsList(string access_token)
        {
            InputCredentialsList credentialsList = new InputCredentialsList()
            {
                clientData = "",
                maxResults = "",
                pageToken = "",
                userID = ""
            };

            RestRequest request = new RestRequest("credentials/list", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = "{}";
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);
            return data;
        }

    }
}