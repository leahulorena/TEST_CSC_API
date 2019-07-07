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
        IAccessToken _accessToken;

        public CredentialsInfoController(IConfiguration configuration, IAccessToken accessToken)
        {
            _configuration = configuration;
            _accessToken = accessToken;
        }

        [HttpPost]
        public object CredentialsInfo(InputCredentialsInfo inputCredentialsInfo)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;


            CredentialsInfoClient credentialsInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);
            object response = credentialsInfoClient.GetCredentialsInfo(_accessToken.GetAccessToken().access_token, inputCredentialsInfo);

            return response;
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