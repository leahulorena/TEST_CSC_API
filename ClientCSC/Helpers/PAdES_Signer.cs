using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SBPDF;
using SBPDFSecurity;
using SBX509;
using SBCustomCertStorage;
using SBPAdES;
using ClientCSC.Models;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;

namespace ClientCSC.Helpers
{
    public class PAdES_Signer
    {
        //nu stiu daca o sa ramana variabilele alea globale...
        private string pin;
        private string otp;
        private string access_token;
        private string credentialsID;
        private string baseURL;
        private string hashAlgo;
        private string signAlgo;

        public bool SignPDF(Stream stream, string accessToken, string OTP, string PIN, string credentialID,string base_URL, string hash_algo, string sign_algo)
        {
            SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");

            pin = PIN; otp = OTP; credentialsID = credentialID; access_token = accessToken; baseURL = base_URL; hashAlgo = hash_algo; signAlgo = sign_algo;
            TElPDFDocument document = new TElPDFDocument();
            try
            {
                document.Open(stream);
                try
                {
                    int index = document.AddSignature();

                    TElPDFSignature signature = document.get_Signatures(index);

                    signature.SigningTime = DateTime.Now;

                    TElPDFAdvancedPublicKeySecurityHandler handler = new TElPDFAdvancedPublicKeySecurityHandler();

                    handler.CustomName = "Adobe.PPKLite";
                    handler.IgnoreChainValidationErrors = true;
                    handler.CertStorage = LoadCertificate(credentialsID, access_token);
                    handler.PAdESSignatureType = TSBPAdESSignatureType.pastEnhanced;
                    handler.HashAlgorithm = SBConstants.__Global.SB_ALGORITHM_DGST_SHA256;
                    handler.RemoteSigningMode = true;
                    handler.RemoteSigningCertIndex = 0;
                    handler.SignatureType = TSBPDFPublicKeySignatureType.pstPKCS7SHA1;
                    handler.SignatureSizeEstimationStrategy = TSBPAdESSignatureSizeEstimationStrategy.psesSmart;
                    handler.OnRemoteSign += new TSBPDFRemoteSignEvent(PAdESHandler_OnRemoteSign);

                    signature.Handler = handler;

                    document.Close(true);

                    return true;
                    //pe document.close se salveaza fisierul in temp path. ar trebui sa citesc iar streamul de acolo si sa il returnez
                }
                catch(Exception ex) { return false; }
            }
            catch(Exception ex) { return false;}


        }


        private TElMemoryCertStorage LoadCertificate(string credentialsID, string access_token)
        {

            //credentialsInfo
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            CredentialsInfoClient credInfoClient = new CredentialsInfoClient(serializer, errorLogger, "http://localhost:8080/api/");

            InputCredentialsInfo credentialsInfo = new InputCredentialsInfo() { credentialID = credentialsID };
            string temp = credInfoClient.GetCredentialsInfo(access_token, credentialsInfo).ToString();

           // string temp = serializer.Serialize(outputCredentials);
            if (!temp.Contains("error") && temp != "")
            {
                OutputCredentialsInfo output = serializer.Deserialize<OutputCredentialsInfo>(temp);

                string certificate = output.cert.certificates.FirstOrDefault();

                TElX509Certificate cert = new TElX509Certificate();
                byte[] certBuf = Encoding.UTF8.GetBytes(certificate);
                int r = cert.LoadFromBufferAuto(certBuf, 0, certBuf.Length, "");

                if (r != 0)
                {
                    throw new Exception("Certificate read error: " + r.ToString());
                }

                TElMemoryCertStorage storage = new TElMemoryCertStorage();
                storage.Add(cert, true);

                return storage;
            }
            else return null;
        }

        private void PAdESHandler_OnRemoteSign(object Sender, byte[] Hash, ref byte[] SignedHash)
        {
            
            string[] hashToSign = new[] { Convert.ToBase64String(Hash) };
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();

            AuthorizeSignClient authorizeSignClient = new AuthorizeSignClient(serializer, errorLogger, baseURL);
            InputAuthorizeSignHash inputAuthorizeSignHash = new InputAuthorizeSignHash()
            {
                credentialsID = credentialsID,
                hash = hashToSign,
                hashAlgo = hashAlgo,
                signAlgo = signAlgo,
                numSignatures = 1,
                OTP = otp,
                PIN = pin
            };

           var outputAuthSign = serializer.Serialize(authorizeSignClient.GetSignedHash(access_token, inputAuthorizeSignHash));

            var signature = serializer.Deserialize<OutputSignaturesSignHash>(outputAuthSign);

            SignedHash = Encoding.UTF8.GetBytes(signature.signatures.FirstOrDefault());
        }
    }

    public class CredentialsInfoClient : BaseClient
    {
        public CredentialsInfoClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetCredentialsInfo(string access_token, InputCredentialsInfo inputCredentialsInfo)
        {

            RestRequest request = new RestRequest("credentialsinfo", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputCredentialsInfo);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
           // var data = serializer.Deserialize<object>(response);

            return response.Content;
        }

    }

    public class AuthorizeSignClient : BaseClient
    {
        public AuthorizeSignClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL) :
            base(serializer, errorLogger, baseURL)
        { }

        public object GetSignedHash(string access_token, InputAuthorizeSignHash inputAuthorizeSignHash)
        {
            RestRequest request = new RestRequest("sign", Method.POST);
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);

            JsonSerializer serializer = new JsonSerializer();
            var postData = serializer.Serialize(inputAuthorizeSignHash);
            request.AddJsonBody(postData);

            IRestResponse response = Execute(request);
            var data = serializer.Deserialize<object>(response);

            return data;
        }
    }
}
