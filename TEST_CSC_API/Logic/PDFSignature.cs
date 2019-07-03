using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SBPDF;
using SBPDFSecurity;
using SBX509;
using SBCustomCertStorage;
using System.Text;
using SBPAdES;
using SBPublicKeyCrypto;
using SBWinCertStorage;
using TEST_CSC_API.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace TEST_CSC_API.Logic
{
    public class PDFSignature
    {

        private IConfiguration configuration;
        private string OTP;
        private string PIN;
        private string Access_token;
        public void SignFilePDF(FileStream file, string access_token, string otp, string pin)
        {
            SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");


            TElPDFDocument doc = new TElPDFDocument();

            try
            {
                doc.Open(file);
                try
                {



                    OTP = otp;
                    PIN = pin;
                    Access_token = access_token;
                   


                    //// doc.Open(docStream);
                    SBCustomCertStorage.TElCustomCertStorage certChain;


                    try
                    {
                        int index = doc.AddSignature();

                        TElPDFSignature sig = doc.get_Signatures(index);

                        sig.SigningTime = DateTime.UtcNow;

                        TElPDFAdvancedPublicKeySecurityHandler handler = new TElPDFAdvancedPublicKeySecurityHandler();

                        handler.CustomName = "Adobe.PPKLite";
                        handler.IgnoreChainValidationErrors = true;
                        handler.CertStorage = LoadCertificate("863971CBC7BF63D49C9F14809FD5A1142B75E9AB", Access_token);
                        handler.PAdESSignatureType = TSBPAdESSignatureType.pastEnhanced;
                        //handler.OfflineMode = true;
                        handler.HashAlgorithm = SBConstants.__Global.SB_ALGORITHM_DGST_SHA256;
                        handler.RemoteSigningMode = true;
                        handler.RemoteSigningCertIndex = 0;
                        handler.SignatureType = TSBPDFPublicKeySignatureType.pstPKCS7SHA1;
                        handler.SignatureSizeEstimationStrategy = TSBPAdESSignatureSizeEstimationStrategy.psesSmart;
                        handler.OnBeforeSign += new TSBPDFSignEvent(OnBefore);
                        handler.OnRemoteSign += new TSBPDFRemoteSignEvent(PAdESHandler_OnRemoteSign);
                        handler.OnAfterSign += new TSBPDFSignEvent(OnAfter);

                        sig.Handler = handler;

                        doc.Close(true);
                    }
                    catch (Exception ex) { }
                }
                catch (Exception ex) { }
            }
            catch (Exception ex) { }
        }

        private void OnBefore(object Sender, SBCMS.TElSignedCMSMessage signedCMSMessage)
        {
            signedCMSMessage.AddSignature();
            signedCMSMessage.GetHashCode();
            signedCMSMessage.get_Signatures(0);
            signedCMSMessage.GetSignature(0);
            signedCMSMessage.GetType();
            signedCMSMessage.Close();
        }

        private void OnAfter(object Sender, SBCMS.TElSignedCMSMessage signedCMSMessage)
        {
            signedCMSMessage.GetHashCode();
            signedCMSMessage.get_Signatures(0);
            signedCMSMessage.GetSignature(0);
            signedCMSMessage.GetType();
        }

        private void PAdESHandler_OnRemoteSign(object Sender, byte[] Hash, ref byte[] SignedHash)
        {
            //aici trebuie sa fac requestul catre transsped si sa intor ref byte[] SignedHash
            //pentru semnare -
            //send otp 
            //credentials authorize - cred id, otp, pin, hash
            //signHash

            //credentials authorize
            string[] hashToSign = new[] { Convert.ToBase64String(Hash) };
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = "https://msign-test.transsped.ro/csc/v0/";
            CredentialsAuthorizeClient credAuth = new CredentialsAuthorizeClient(serializer, errorLogger, baseURL);
            InputCredentialsAuthorize inputCredAuth = new InputCredentialsAuthorize() { credentialID = "863971CBC7BF63D49C9F14809FD5A1142B75E9AB", hash = hashToSign, OTP = OTP, PIN = PIN, numSignatures = 1 };
            string outputCredAuth = serializer.Serialize(credAuth.GetCredentialsAuthorize(Access_token, inputCredAuth));
            OutputCredentialsAuthorize authorize = serializer.Deserialize<OutputCredentialsAuthorize>(outputCredAuth);


            //sign hash

            InputSignaturesSignHash inputSignHash = new InputSignaturesSignHash() { credentialID = "863971CBC7BF63D49C9F14809FD5A1142B75E9AB", hash = hashToSign, SAD = authorize.SAD, hashAlgo = "2.16.840.1.101.3.4.2.1", signAlgo = "1.2.840.113549.1.1.11" };
            SignHashClient signHashClient = new SignHashClient(serializer, errorLogger, baseURL);
            string outputSignature = serializer.Serialize(signHashClient.GetSignedHash(Access_token, inputSignHash));
            OutputSignaturesSignHash signature = serializer.Deserialize<OutputSignaturesSignHash>(outputSignature);

            SignedHash = Encoding.UTF8.GetBytes(signature.signatures.FirstOrDefault());
        }


        private TElMemoryCertStorage LoadCertificate(string credentialsID, string access_token)
        {

            //credentialsInfo
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            CredentialsInfoClient credInfoClient = new CredentialsInfoClient(serializer, errorLogger, "https://msign-test.transsped.ro/csc/v0/");

            InputCredentialsInfo credentialsInfo = new InputCredentialsInfo() { credentialID = credentialsID };
            object outputCredentials = credInfoClient.GetCredentialsInfo(access_token, credentialsInfo);

            string temp = serializer.Serialize(outputCredentials);
            if (!temp.Contains("error"))
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


    }
}

