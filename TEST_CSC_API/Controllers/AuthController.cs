using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RestSharp;
using RestSharp.Deserializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("authorize")]
        public void GetAuth()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();

            string client_id = _configuration.GetSection("Transsped").GetSection("ClientID").Value;
            string client_secret = _configuration.GetSection("Transsped").GetSection("ClientSecret").Value;
            string redirect_uri = _configuration.GetSection("Transsped").GetSection("RedirectURL").Value;
            string auth_uri = _configuration.GetSection("Transsped").GetSection("AuthURL").Value;

         
            RestRequest request = new RestRequest(auth_uri, Method.GET);
            RestClient client = new RestClient();
            Parameter parameter_id = new Parameter("client_id", client_id, ParameterType.UrlSegment);
            Parameter parameter_secret = new Parameter("client_secret", client_secret, ParameterType.UrlSegment);
            Parameter parameter_redirect = new Parameter("redirect_uri", redirect_uri, ParameterType.UrlSegment);
            Parameter parameter_token = new Parameter("token_type", "token", ParameterType.UrlSegment);
            request.AddParameter(parameter_id);
            request.AddParameter(parameter_secret);
            request.AddParameter(parameter_redirect);
            request.AddParameter(parameter_token);
            
            
           

        }

        [HttpPost]
        public string OAuthToken()
        {
            //ar trebui sa citesc parametrii pentru oauth din fisierul de configurare sau sa ii primesc ca parametru?
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();

            string code = _configuration.GetSection("Transsped").GetSection("Code").Value;
            string client_id = _configuration.GetSection("Transsped").GetSection("ClientID").Value;
            string client_secret = _configuration.GetSection("Transsped").GetSection("ClientSecret").Value;
            string redirect_uri = _configuration.GetSection("Transsped").GetSection("RedirectURL").Value;
            string token_url = _configuration.GetSection("Transsped").GetSection("TokenURL").Value;
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            InputOAuth2Token inputOAuth2Token = new InputOAuth2Token();
            inputOAuth2Token.client_id = client_id;
            inputOAuth2Token.client_secret = client_secret;
            inputOAuth2Token.code = code;
            inputOAuth2Token.redirect_uri = redirect_uri;
            inputOAuth2Token.grant_type = "authorization_code";

            var postData = serializer.Serialize(inputOAuth2Token);



            OAuth2Client oAuth2 = new OAuth2Client(serializer, errorLogger, baseURL);

            object output = oAuth2.GetOauth2Token(token_url, postData);

            return serializer.Serialize(output);
        }
    }
}