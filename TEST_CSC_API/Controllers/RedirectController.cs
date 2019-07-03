using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TEST_CSC_API.Controllers
{
    public class RedirectController : Controller
    {

        IConfiguration _configuration;

        public RedirectController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var code = this.Request.QueryString.ToUriComponent();            
            return View();
        }
    }
}