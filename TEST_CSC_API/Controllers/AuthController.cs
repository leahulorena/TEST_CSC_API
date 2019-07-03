using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public void Auth()
        {
            //OAuth2Controller oAuth2 = new OAuth2Controller(_configuration);
            //oAuth2.Login();
            //string code = _configuration.GetSection("Transsped").GetSection("Code").Value;
            //while (code == null)
            //{
            //    string result = oAuth2.OAuthToken();
            //}
        }
    }
}