using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    public class CredentialsExtendTransactionController : ControllerBase
    {
        IConfiguration _configuration;

        public CredentialsExtendTransactionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public string ExtendTransaction(InputCredentialsExtendTransaction inputCredentialsExtend)
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


            ExtendTransactionClient extendTransactionClient = new ExtendTransactionClient(serializer, errorLogger, baseURL);
            object response = extendTransactionClient.GetExtendTransaction(access_token, inputCredentialsExtend);

            return serializer.Serialize(response);

        }
    }

    public class ExtendTransactionClient : BaseClient
    {
        public ExtendTransactionClient(IDeserializer serializer, ErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetExtendTransaction(string access_token, InputCredentialsExtendTransaction inputCredentialsExtend)
        {
           
            RestRequest request = new RestRequest("credentials/extendTransaction", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsExtend);
            request.AddJsonBody(postData);

            IRestResponse response = Execute<object>(request);
            var data = serializer.Deserialize<object>(response);

            return data;

        }
    }
}