using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Deserializers;
using ClientCSC.Models;
using ClientCSC.Helpers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace ClientCSC.Controllers
{
    public class SignController : Controller
    {
        IConfiguration _configuration;
        IAccessToken _accessToken;

        public SignController(IConfiguration configuration, IAccessToken accessToken)
        {
            _configuration = configuration;
            _accessToken = accessToken;
        }


        public IActionResult Index()
        {
            JsonSerializer serializer = new JsonSerializer();

            string access_token = GetAccessToken();
            OutputOauth2Token outputOauth = serializer.Deserialize<OutputOauth2Token>(access_token);
            _accessToken.SetAccessToken(outputOauth.access_token, outputOauth.token_type, outputOauth.expires_in);

            return View();
        }


        //get access token
        public string GetAccessToken()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("CSC_API").GetSection("BaseURL").Value;
            MyHttpClient myHttpClient = new MyHttpClient(serializer, errorLogger, baseURL);
            var accessTokenResponse = myHttpClient.GetAccessToken();

            return accessTokenResponse;
        }

        public ActionResult _LoadCert()
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("CSC_API").GetSection("BaseURL").Value;

            InputCredentialsList credentialsList = new InputCredentialsList() { };
            MyHttpClient myHttpClient = new MyHttpClient(serializer, errorLogger, baseURL);
            var response = myHttpClient.GetCertificatesList(_accessToken.GetAccessToken().access_token, credentialsList);
            List<CredentialObject> userCertificates = new List<CredentialObject>();
            if (!response.Contains("error"))
            {
                List<OutputCredentials> outputCredentials = serializer.Deserialize<List<OutputCredentials>>(response);
                foreach (var output in outputCredentials)
                {
                    //trebuie sa adaug si credential id
                    Cert certificate = new Cert();
                    byte[] certBytes = Convert.FromBase64String(output.cert.certificates.FirstOrDefault());
                    var certTest = new X509Certificate2(certBytes);
                    certificate.issuerDN = certTest.IssuerName.Name.ToString();
                    certificate.serialNumber = certTest.SerialNumber.ToString();
                    certificate.subjectDN = certTest.SubjectName.Name.ToString();
                    certificate.status = certTest.FriendlyName.ToString();
                    certificate.validFrom = certTest.NotBefore.ToString();
                    certificate.validTo = certTest.NotAfter.ToString();

                    string credID = output.credentialID;

                    CredentialObject userCredentialObject = new CredentialObject();
                    userCredentialObject.credentialID = credID;
                    userCredentialObject.certificate = certificate;

                    userCertificates.Add(userCredentialObject);

                }
            }
            return PartialView("_LoadCert", userCertificates);
        }

        public string SendOTP(string credentialID)
        {
            InputCredentialsSendOTP credentialsSendOTP = new InputCredentialsSendOTP()
            {
                credentialID = credentialID
            };
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("CSC_API").GetSection("BaseURL").Value;

            MyHttpClient myHttpClient = new MyHttpClient(serializer, errorLogger, baseURL);
            var response = myHttpClient.SendOTP(_accessToken.GetAccessToken().access_token, credentialsSendOTP);
            if (response == null || response.Contains("error"))
            {
                return "fail";
            }
            else
            {
                return "ok";
            }
        }

        public async Task<IActionResult> SignData(SignatureModel data, int? cades)
        {
            try
            {

                var filePath = Path.GetTempFileName();
                if (data.inputFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        //METODELE DE SEMNARE DIN API
                        await data.inputFile.CopyToAsync(stream);

                        MemoryStream memoryStream = new MemoryStream();
                        await data.inputFile.CopyToAsync(memoryStream);
                        string hash_algorithm = ""; string sign_algorithm = ""; int signature_type;
                        if(data.algorithm == 1)
                        {
                            hash_algorithm = "2.16.840.1.101.3.4.2.1";
                            sign_algorithm = "1.2.840.113549.1.1.11";
                        }
                        else
                        {
                            hash_algorithm = "1.3.14.3.2.26";
                            sign_algorithm = "1.3.14.3.2.29";
                        }
                        if (data.inputFile.ContentType == "application/pdf")
                        {
                            signature_type = 1;
                        }
                        else if(data.inputFile.ContentType == "text/xml")
                        {
                            signature_type = 2;
                        }
                        else
                        {
                            signature_type = 3;
                        }

                        if(cades == 1)
                        {
                            signature_type = 3;
                        }
                        InputSignatureAdvanced inputSignatureAdvanced = new InputSignatureAdvanced()
                        {
                            credentialsID = data.credentialsID,
                            hashAlgo = hash_algorithm,
                            signAlgo = sign_algorithm,
                            OTP = data.otp,
                            PIN = data.pin,
                            signatureType = signature_type,
                            documentStream = memoryStream.GetBuffer()

                        };
                        var ceva = Encoding.UTF8.GetBytes(memoryStream.GetBuffer().ToString());
                        JsonSerializer serializer = new JsonSerializer();
                        ErrorLogger errorLogger = new ErrorLogger();
                        string baseURL = _configuration.GetSection("CSC_API").GetSection("BaseURL").Value;

                        MyHttpClient myHttpClient = new MyHttpClient(serializer, errorLogger, baseURL);
                        var response = myHttpClient.PAdES(_accessToken.GetAccessToken().access_token, inputSignatureAdvanced);
                        if (response == null || response.Contains("error"))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            //eu primesc byte array
                            OutputAdvancedSignature output = serializer.Deserialize<OutputAdvancedSignature>(response);

                            MemoryStream signedMemory = new MemoryStream(output.signedDocument);

                            signedMemory.Position = 0;
                            if (signature_type == 1)
                            {
                                return File(signedMemory, "application/pdf", "signed-pdf.pdf");
                            }
                            else if (signature_type == 2)
                            {
                                return File(signedMemory, "text/xml", "signed-xml.xml");
                            }
                            else
                            {
                                return File(signedMemory, "application/pkcs7-signature", "signed-cms.p7s");
                            }


                        }
                        //METODELE DE SEMNARE DIN CLIENT

                        // bool flag = false;
                        // MemoryStream memoryxml = new MemoryStream();
                        //if (cades == 1)
                        //{
                        //    MemoryStream memory = new MemoryStream();
                        //    if (data.algorithm == 1)
                        //    {
                        //        //SHA256 RSA
                        //        memory = SBBSignCMS(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                        //    }
                        //    else
                        //    {
                        //        //SHA1 RSA
                        //        memory = SBBSignCMS(stream, data.credentialsID, "1.3.14.3.2.26", "1.3.14.3.2.29", data.otp, data.pin);
                        //    }
                        //    if (memory != null)
                        //    {
                        //        memory.Position = 0;
                        //        return File(memory, "application/pkcs7-signature", "test.p7s");
                        //    }
                        //}
                        //else
                        //{
                        //    if (data.inputFile.ContentType == "application/pdf")
                        //    {
                        //        if (data.algorithm == 1)
                        //        {
                        //            //SHA256 RSA
                        //            flag = SBBSignPDF(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                        //        }
                        //        else
                        //        {
                        //            //SHA1 RSA
                        //            flag = SBBSignPDF(stream, data.credentialsID, "1.3.14.3.2.26", "1.3.14.3.2.29", data.otp, data.pin);

                        //        }
                        //        stream.Close();
                        //        stream.Dispose();
                        //    }
                        //    else if (data.inputFile.ContentType == "text/xml")
                        //    {
                        //        if (data.algorithm == 1)
                        //        {
                        //            //SHA
                        //            memoryxml = SBBSignXML(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                        //        }
                        //        else
                        //        {

                        //            memoryxml = SBBSignXML(stream, data.credentialsID, "1.3.14.3.2.26", "1.3.14.3.2.29", data.otp, data.pin);

                        //        }
                        //    }

                        //    if (flag == true)
                        //    {
                        //        var memory = new MemoryStream();

                        //        using (FileStream signedStrem = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        //        {
                        //            await signedStrem.CopyToAsync(memory);
                        //        }
                        //        memory.Position = 0;
                        //        return File(memory, "application/pdf", "lorena-signed.pdf");
                        //    }

                        //    if (memoryxml != null)
                        //    {

                        //        memoryxml.Position = 0;
                        //        return File(memoryxml, "text/xml", "lorena-signed.xml");
                        //    }
                        //}
                    }

                }


                else { return RedirectToAction("Index"); }
                return RedirectToAction("Index");
            }
            catch (Exception ex) { return RedirectToAction("Index"); }
        }

        public bool SBBSignPDF(Stream fileStream, string credentialsID, string hashAlgo, string signAlgo, string otp, string pin)
        {
            PAdES_Signer pAdES_Signer = new PAdES_Signer();
            bool flag = pAdES_Signer.SignPDF(fileStream, _accessToken.GetAccessToken().access_token, otp, pin, credentialsID, _configuration.GetSection("CSC_API").GetSection("BaseURL").Value, hashAlgo, signAlgo);
            return flag;
        }

        public MemoryStream SBBSignXML(Stream fileStream, string credentialsID, string hashAlgo, string signAlgo, string otp, string pin)
        {
            XAdES_Signer xAdES_Signer = new XAdES_Signer();
            MemoryStream memory = xAdES_Signer.SignXML(fileStream, _accessToken.GetAccessToken().access_token, otp, pin, credentialsID, _configuration.GetSection("CSC_API").GetSection("BaseURL").Value, hashAlgo, signAlgo);
            return memory;
        }

        public MemoryStream SBBSignCMS(Stream fileStream, string credentialsID, string hashAlgo, string signAlgo, string otp, string pin)
        {
            CAdES_Signer cAdES_Signer = new CAdES_Signer();
            MemoryStream memory = cAdES_Signer.SignCMS(fileStream, _accessToken.GetAccessToken().access_token, otp, pin, credentialsID, _configuration.GetSection("CSC_API").GetSection("BaseURL").Value, hashAlgo, signAlgo);
            return memory;
        }
    }

    public class MyHttpClient : BaseClient
    {
        public MyHttpClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public string GetAccessToken()

        {
            RestRequest request = new RestRequest("csc_api/auth", Method.GET);
            IRestResponse response = Execute(request);
            return response.Content;
        }


        public string GetCertificatesList(string access_token, InputCredentialsList inputCredentialsList)
        {
            RestRequest request = new RestRequest("csc_api/credentials", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsList);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            return response.Content;
        }

        public string SendOTP(string access_token, InputCredentialsSendOTP sendOTP)
        {
            RestRequest request = new RestRequest("csc_api/otp", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(sendOTP);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            return response.Content;
        }

        public string PAdES(string access_token, InputSignatureAdvanced inputSignatureAdvanced)
        {
            RestRequest request = new RestRequest("advancedsign", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputSignatureAdvanced);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            return response.Content;
        }
    }
}