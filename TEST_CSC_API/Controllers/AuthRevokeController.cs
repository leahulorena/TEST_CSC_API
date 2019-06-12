using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AuthRevokeController : ControllerBase
    {
        IConfiguration _configuration;

        public AuthRevokeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public string AuthRevoke()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;
            AuthRevokeClient authRevokeClient = new AuthRevokeClient(serializer, errorLogger, baseURL);
            object response = authRevokeClient.GetAuthRevoke("6445f209-d5fd-4fa1-aceb-2e1f556f2840");

            return serializer.Serialize(response);
        }
    }

    public class AuthRevokeClient : BaseClient
    {
        public AuthRevokeClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }


        public object GetAuthRevoke(string access_token)
        {
            InputAuthRevoke inputAuthRevoke = new InputAuthRevoke()
            {
                clientData = "",
                token = access_token
            };

            RestRequest request = new RestRequest("auth/revoke", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputAuthRevoke);
            request.AddJsonBody(postData);

            IRestResponse response = Execute<object>(request);
            var data = serializer.Deserialize<object>(response);

            return data;
        }
    }
}