using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TEST_CSC_API.Logic;

namespace TEST_CSC_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdvancedSign : ControllerBase
    {
        IConfiguration _configuration;

        public AdvancedSign(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //tot byte array intorc
        [HttpPost]
        public async Task<object> Sign(InputSignatureAdvanced inputSignatureAdvanced)
        {
            OutputError error = new OutputError()
            {
                error = "invalid_access_token",
                error_description = "Invalid access_token"
            };
            //aici, in functie de tipul de signature type apelez metodele separate de pdf, xml, cms
            Microsoft.Extensions.Primitives.StringValues value;
            string access_token = "";
            if (Request.Headers.TryGetValue("Authorization", out value))
            {
                access_token = value.ToString().Replace("Bearer ", "");
            }
            else
            {
                error.error_description = "access token nu poate fi extras";
                return error;
            }

            if (inputSignatureAdvanced == null)
            {
                error.error_description = "parametrii null";
                return error;
            }
            else
            {

                switch (inputSignatureAdvanced.signatureType)
                {
                    case 1:
                        {
                            PAdES_Logic pades = new PAdES_Logic();
                            var output = await pades.SignPDFAsync(access_token, _configuration.GetSection("Transsped").GetSection("BaseURL").Value, inputSignatureAdvanced);
                            error.error_description = "error on cades";
                            return output;
                        }
                    case 2:
                        {
                            XAdES_Logic xades = new XAdES_Logic();
                            var output = await xades.SignXMLAsync(access_token, _configuration.GetSection("Transsped").GetSection("BaseURL").Value, inputSignatureAdvanced);
                            error.error_description = "error on xades";
                            return output;
                        }
                    case 3:
                        {
                            CAdES_Logic cades = new CAdES_Logic();
                            var output = await cades.SignCMSAsync(access_token, _configuration.GetSection("Transsped").GetSection("BaseURL").Value, inputSignatureAdvanced);
                            error.error_description = "error on cades";
                            return output;
                        }

                    default:
                        {
                            error.error_description = "caz ciudat";
                            return error;
                        }
                }

            }
        }
    }
}