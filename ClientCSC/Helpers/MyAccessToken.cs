using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientCSC.Models;

namespace ClientCSC.Helpers
{
    public interface IAccessToken
    {
        void SetAccessToken(string access_token, string token_type, int expires_in);
        OutputOauth2Token GetAccessToken();

    }

    public class MyAccessToken : IAccessToken
    {
        private OutputOauth2Token AccessToken;

        public void SetAccessToken(string access_token, string token_type, int expires_in)
        {
            AccessToken = new OutputOauth2Token();
            AccessToken.access_token = access_token;
            AccessToken.token_type = token_type;
            AccessToken.expires_in = expires_in;
        }
        public OutputOauth2Token GetAccessToken()
        {
            return AccessToken;
        }
    }
}
