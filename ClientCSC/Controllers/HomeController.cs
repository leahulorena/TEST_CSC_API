using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

using ClientCSC.Models;
using ClientCSC.Helpers;

namespace ClientCSC.Controllers
{
    public class HomeController : Controller
    {
        IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
            
        [EnableCors]
        public IActionResult Index(bool? flag)
        {
            if(flag ==null || flag == false)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Privacy");
            }
        }


    
        public IActionResult Privacy()
        {

            var token_url = _configuration.GetSection("CSC_API").GetSection("TokenURL").Value;
            TempData["access_token"] = "afafadfad";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
