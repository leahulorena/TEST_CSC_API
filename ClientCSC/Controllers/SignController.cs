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
            if (response != null)
            {
                return "fail";
            }
            else
            {
                return "ok";
            }
        }


        //in functie de ce tip de fisier mi se incarca sa fac diferentierea intre PAdES si XAdES
        public async Task<IActionResult> SignData(SignatureModel data)
        {
            try
            {
                bool flag = false;
                MemoryStream memoryxml = new MemoryStream();

                var filePath = Path.GetTempFileName();
                if (data.inputFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await data.inputFile.CopyToAsync(stream);

                        //sha256 \"signAlgo\": \"1.2.840.113549.1.1.11\", \"hashAlgo\": \"2.16.840.1.101.3.4.2.1\"
                        //sha1 \"signAlgo\": \"1.3.14.3.2.29\", \"hashAlgo\": \"1.3.14.3.2.26\

                        //teoretic ar cam trebui sa intorc streamul, sa vad daca reusesc asa
                        if (data.inputFile.ContentType == "application/pdf")
                        {
                            if (data.algorithm == 1)
                            {
                                flag = SBBSignPDF(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);
                                // flag = SBBSignXML(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                            }
                            else
                            {
                                flag = SBBSignPDF(stream, data.credentialsID, "1.3.14.3.2.26", "1.3.14.3.2.29", data.otp, data.pin);
                                //flag = SBBSignXML(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                            }
                            //sa nu uitam de access token !
                            stream.Close();
                            stream.Dispose();
                        }
                        else if (data.inputFile.ContentType == "text/xml")
                        {
                            if (data.algorithm == 1)
                            {

                                memoryxml = SBBSignXML(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                            }
                            else
                            {

                                memoryxml = SBBSignXML(stream, data.credentialsID, "2.16.840.1.101.3.4.2.1", "1.2.840.113549.1.1.11", data.otp, data.pin);

                            }
                        }

                        if (flag == true)
                        {
                            var memory = new MemoryStream();

                            using (FileStream signedStrem = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                //return File(signedStrem, "application/octet-stream");
                                await signedStrem.CopyToAsync(memory);
                            }
                            memory.Position = 0;
                            //text/xml, ceva signed.xml
                            return File(memory, "application/pdf", "lorena-signed.pdf");
                        }

                        if(memoryxml != null)
                        {
                            //using (FileStream signedStrem = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            //{
                            //    //return File(signedStrem, "application/octet-stream");
                            //    await signedStrem.CopyToAsync(memoryxml);
                            //}
                            memoryxml.Position = 0;
                            //text/xml, ceva signed.xml
                            return File(memoryxml, "text/xml", "lorena-signed.xml");
                        }
                    }

                }


                else { return null; }
                return null;
            }
            catch (Exception ex) { return null; }
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
    }

    public class MyHttpClient : BaseClient
    {
        public MyHttpClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public string GetAccessToken()

        {
            RestRequest request = new RestRequest("auth", Method.GET);
            IRestResponse response = Execute(request);
            return response.Content;
        }


        public string GetCertificatesList(string access_token, InputCredentialsList inputCredentialsList)
        {
            RestRequest request = new RestRequest("credentials", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsList);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            return response.Content;
        }

        public string SendOTP(string access_token, InputCredentialsSendOTP sendOTP)
        {
            RestRequest request = new RestRequest("otp", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(sendOTP);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            return response.Content;
        }


    }
}