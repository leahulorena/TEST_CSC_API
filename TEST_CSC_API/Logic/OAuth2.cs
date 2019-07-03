using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;


namespace TEST_CSC_API.Logic
{
    public class OAuth2
    {
        IConfiguration _configuration;

        public OAuth2(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string OAuthToken(string code)
        {
            //ar trebui sa citesc parametrii pentru oauth din fisierul de configurare sau sa ii primesc ca parametru?
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();

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

    public class OAuth2Client : BaseClient
    {
        public OAuth2Client(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
           base(serializer, errorLogger, baseURL)
        { }

        public object GetOauth2Token(string token_url, string postData)
        {
            RestRequest request = new RestRequest("oauth2/token", Method.POST);
            request.AddJsonBody(postData);
            IRestResponse response = Execute(request);
            JsonSerializer serializer = new JsonSerializer();
            var data = serializer.Deserialize<object>(response);
            return data;

        }
    }
}
