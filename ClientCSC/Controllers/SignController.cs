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
            List<Cert> userCertificates = new List<Cert>();
            if (!response.Contains("error"))
            {
                List<OutputCredentialsInfo> outputCredentials = serializer.Deserialize<List<OutputCredentialsInfo>>(response);
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
                    userCertificates.Add(certificate);
                }
            }
            return PartialView("_LoadCert", userCertificates);
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

    }
}