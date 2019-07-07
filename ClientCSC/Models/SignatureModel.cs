using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientCSC.Models
{
    public class SignatureModel
    {
        public string pin { get; set; }
        public string otp { get; set; }

        public string credentialsID { get; set; }

        public IFormFile inputFile { get; set; }

        public IFormFile outputFile { get; set; }

        public int algorithm { get; set; }
    }
}
