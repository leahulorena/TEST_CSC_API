using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TEST_CSC_API.Logic;


namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignPDFController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        IConfiguration _configuration;

        public SignPDFController(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> PostFile([FromForm]IFormFile file, [FromForm]string OTP, [FromForm]string pin)
        {


            var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Documents");
            var filePath = Path.Combine(uploads, file.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyToAsync(fileStream).Wait();

             

                Microsoft.Extensions.Primitives.StringValues value;
                string access_token = "";
                if (Request.Headers.TryGetValue("Authorization", out value))
                {
                    access_token = value.ToString().Replace("Bearer ", "");
                }
                else
                {
                    OutputError error = new OutputError()
                    {
                        error = "invalid_access_token",
                        error_description = "Invalid access_token"
                    };

                }

                PDFSignature pdfSign = new PDFSignature();
                pdfSign.SignFilePDF(fileStream, access_token, OTP, pin);

                //imi salveaza documentul local
                //trebuie sa vad cum trimit documentul inapoi la user

            }
            //System.IO.File.Copy(file.Name, @"E:\Dezvoltare\test.pdf", true);

            //System.IO.FileStream F = new FileStream(@"E:\Dezvoltare\test.pdf", FileMode.Open, FileAccess.ReadWrite);
            //try()
            //var stream = file.OpenReadStream();
            //var name = file.FileName;
            //var type = file.ContentType;
           
            return null;
        }

    }

}