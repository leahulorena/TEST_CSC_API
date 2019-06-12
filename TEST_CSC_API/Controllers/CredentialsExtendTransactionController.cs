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

        [HttpGet]
        public string ExtendTransaction()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;


            byte[] bytes = Encoding.UTF8.GetBytes("text to hash");
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashBase64 = Convert.ToBase64String(hash);

            string access_token = "1f65bd03-3568-4968-85e1-754cb1c9ede4";
            string sad = "MIHaDChhdC5ndi5lZ2l6LmJrdS5zZXJ2ZXIuY3J5cHRvLldyYXBwZWREYXRhMIGtDBRBRVMvQ0JDL1BLQ1M1UGFkZGluZwQSBBCgbogrlYqBrdmyOOygn9dbBIGAelxl1Mp01lLbSoECzqpjd5BDWqsaFgjKPGOmuUPAET0WT8YDo1wgzSw7thzuVEvaeS4+oCgIks7W29uwKq1tVTKwlV2P1dPC9r/vvTMvBiWS63CWL15aP+t8L1ywZhXIiAzJTG/AdqflnN2mrCRGyNF7BPpIMizf1fZPUYryVPI=";
            string credentialsID = "863971CBC7BF63D49C9F14809FD5A1142B75E9AB";


            ExtendTransactionClient extendTransactionClient = new ExtendTransactionClient(serializer, errorLogger, baseURL);
            object response = extendTransactionClient.GetExtendTransaction(access_token, credentialsID, sad, hashBase64);

            return serializer.Serialize(response);

        }
    }

    public class ExtendTransactionClient : BaseClient
    {
        public ExtendTransactionClient(IDeserializer serializer, ErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetExtendTransaction(string access_token, string credentialID, string sad, string hash)
        {
            InputCredentialsExtendTransaction inputCredentialsExtend = new InputCredentialsExtendTransaction()
            {
                clientData = "",
                credentialID = credentialID,
                hash = hash,
                SAD = sad
            };

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