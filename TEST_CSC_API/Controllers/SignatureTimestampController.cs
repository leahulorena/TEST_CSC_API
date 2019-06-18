﻿using System;
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
    public class SignatureTimestampController : ControllerBase
    {
        IConfiguration _configuration;

        public SignatureTimestampController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public string SignatureTimestamp(InputSignaturesTimestamp inputSignaturesTimestamp)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;
            SignatureTimestampClient signTimestampClient = new SignatureTimestampClient(serializer, errorLogger, baseURL);

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

            object response = signTimestampClient.GetSignTimestamp(access_token, inputSignaturesTimestamp);

            return serializer.Serialize(response);
        }
    }

    public class SignatureTimestampClient : BaseClient
    {
        public SignatureTimestampClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetSignTimestamp(string access_token, InputSignaturesTimestamp inputSignaturesTimestamp)
        {
            RestRequest request = new RestRequest("sign/timestamp", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputSignaturesTimestamp);
            request.AddJsonBody(postData);

            IRestResponse response = Execute<object>(request);
            var data = serializer.Deserialize<object>(response);

            return data;
        }
    }
}