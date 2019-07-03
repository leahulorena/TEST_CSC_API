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

namespace TEST_CSC_API
{
    public class RedirectModel : PageModel
    {
        private readonly IWritableOptions<Transsped> _writableLocations;

        public RedirectModel(IWritableOptions<Transsped> writable)
        {
            _writableLocations = writable;
        }

        public void OnGet()
        {
            var response = this.Request.QueryString.ToUriComponent();

            int startIndex = response.IndexOf("code=");
            int endIndex = response.IndexOf("&state");

            string code = response.Substring(startIndex + 5, endIndex - startIndex - 5);

            _writableLocations.Update(options =>
            {
                options.Code = code;
            });



        }
    }
}