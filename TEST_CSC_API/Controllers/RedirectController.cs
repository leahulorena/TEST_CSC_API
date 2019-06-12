using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TEST_CSC_API.Controllers
{
    public class RedirectController : Controller
    {
        public IActionResult Index()
        {
            var code = this.Request.QueryString.ToUriComponent();
            return View();
        }
    }
}