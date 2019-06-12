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
    public class SignHashController : ControllerBase
    {
        IConfiguration _configuration;

        public SignHashController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public string SignHash()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            SignHashClient signHashClient = new SignHashClient(serializer, errorLogger, baseURL);
            object response = signHashClient.GetSignedHash("9508861a-4a19-4e8a-8f4a-6cd78c861366", "863971CBC7BF63D49C9F14809FD5A1142B75E9AB");

            return serializer.Serialize(response);
        }
    }


    public class SignHashClient : BaseClient
    {
        public SignHashClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetSignedHash(string access_token, string credentialID)
        {
            byte[] bytes = Encoding.GetBytes("text to hash");
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashBase64 = Convert.ToBase64String(hash);
            string[] hashToSign = new string[] { hashBase64 };

            InputSignaturesSignHash inputSignaturesSignHash = new InputSignaturesSignHash()
            {
                hash = hashToSign,
                credentialID = credentialID,
                hashAlgo = "2.16.840.1.101.3.4.2.1",
                SAD = "MIIBCgwoYXQuZ3YuZWdpei5ia3Uuc2VydmVyLmNyeXB0by5XcmFwcGVkRGF0YTCB3QwUQUVTL0NCQy9QS0NTNVBhZGRpbmcEEgQQHki2YrS8EvIdOWreqSaSNQSBsDrn2xNQut8kzkBAoPXm7NOcMUDVy01HoEseaCfzc5c/Mb1f9XgY7dbH8Vavd/8/rtBo6SoEAJzvH5uPbtbGnLG1Bajs3fhrlkdCSPjm6+VunnSjZVjm1jxoZmCcqJRFi2WH78yd+WTAVqroIZ/PHNzHXi2iRTdype3UoNZPGf/Gxfuje4gaoMZJQdal6RLJ5vRCLXdASFL0REQJ/gye6HdcK+k7DJH7JGFgeVz0oI9F",
                signAlgo = "1.2.840.113549.1.1.11"

            };


            RestRequest request = new RestRequest("signatures/signHash", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputSignaturesSignHash);


            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;

        }
    }
}