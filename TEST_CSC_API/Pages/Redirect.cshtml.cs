using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RestSharp.Deserializers;
using RestSharp;
using TEST_CSC_API.Logic;
using Microsoft.Extensions.Options;

namespace TEST_CSC_API
{
    public class RedirectModel : PageModel
    {
        private readonly IWritableOptions<Transsped> _writableLocations;
        private IConfiguration _configuration;
        private IAccessToken _accessToken;


        public RedirectModel(IWritableOptions<Transsped> writable, IConfiguration configuration, IAccessToken accessToken)
        {
            _writableLocations = writable;
            _configuration = configuration;
            _accessToken = accessToken;
        }


        public void OnGet()
        {
            var response = this.Request.QueryString.ToUriComponent();

            int startIndex = response.IndexOf("code=");
            int endIndex = response.IndexOf("&state");

            string code = response.Substring(startIndex + 5, endIndex - startIndex - 5);

            var clientAppUrl = _configuration.GetSection("Transsped").GetSection("ClientAppURL").Value;

            TempData["clienturl"] = clientAppUrl;

            GetAccessToken(code);           
        }

        public void GetAccessToken(string code)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();

            string client_id = _configuration.GetSection("Transsped").GetSection("ClientID").Value;
            string client_secret = _configuration.GetSection("Transsped").GetSection("ClientSecret").Value;
            string redirect_uri = _configuration.GetSection("Transsped").GetSection("RedirectURL").Value;
            string token_url = _configuration.GetSection("Transsped").GetSection("TokenURL").Value;
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            OAuth2Client oAuth2Client = new OAuth2Client(serializer, errorLogger, baseURL);
            InputOAuth2Token inputOAuth2Token = new InputOAuth2Token()
            {
                client_id = client_id,
                client_secret = client_secret,
                code = code,
                grant_type = "authorization_code",
                redirect_uri = redirect_uri
            };
            var responseToken = oAuth2Client.GetOauth2Token("token", serializer.Serialize(inputOAuth2Token));

            var serializedResponseToken = serializer.Serialize(responseToken);
            if (!serializedResponseToken.Contains("error"))
            {
                OutputOauth2Token oauth2Token = serializer.Deserialize<OutputOauth2Token>(serializedResponseToken);
                _accessToken.SetAccessToken(oauth2Token.access_token, oauth2Token.token_type, oauth2Token.expires_in);
            }
        }
    }
}