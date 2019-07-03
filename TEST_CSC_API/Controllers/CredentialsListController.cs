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


        [HttpPost]
        public string CredentialsList(InputCredentialsList inputCredentialsList)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsListClient credentialsListClient = new CredentialsListClient(serializer, errorLogger, baseURL);
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
            object response = credentialsListClient.GetCredentialsList(access_token, inputCredentialsList);
            return serializer.Serialize(response);
        }

    }

    public class CredentialsListClient : BaseClient
    {
        public CredentialsListClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
           base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsList(string access_token, InputCredentialsList inputCredentialsList)
        {
            RestRequest request = new RestRequest("credentials/list", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsList);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);
            return data;
        }

    }
}