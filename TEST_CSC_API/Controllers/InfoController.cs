using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using Microsoft.Extensions.Configuration;

namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        IConfiguration _configuration;

        public InfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public string Get()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;
            InfoClient info = new InfoClient(serializer, errorLogger, baseURL);
            object output = info.GetInfo();

            string returnedData = serializer.Serialize(output);
            return returnedData;
        }
    }

    public class InfoClient : BaseClient
    {
        public InfoClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
           base(serializer, errorLogger, baseURL)
        { }

        public OutputInfo GetInfo()
        {
            RestRequest request = new RestRequest("info", Method.POST);

            InputInfo inputInfo = new InputInfo() { lang = "en-US" };
            JsonSerializer jsonSerializer = new JsonSerializer();
            string inputInfoArguments = jsonSerializer.Serialize(inputInfo);

            request.AddJsonBody(inputInfoArguments);
            return Get<OutputInfo>(request);
        }
    }
}