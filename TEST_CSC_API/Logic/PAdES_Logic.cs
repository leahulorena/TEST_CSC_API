using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SBPDF;
using SBPDFSecurity;
using SBX509;
using SBCustomCertStorage;
using SBPAdES;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;
using System.IO;
using TEST_CSC_API;
using TEST_CSC_API.Controllers;

namespace TEST_CSC_API.Logic
{
    public class PAdES_Logic
    {
        private string pin;
        private string otp;
        private string access_token;
        private string credentialsID;
        private string baseURL;
        private string hashAlgo;
        private string signAlgo;
        public async Task<object> SignPDFAsync(string accessToken, string base_URL, InputSignatureAdvanced inputSignatureAdvanced)
        {


            SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");
            pin = inputSignatureAdvanced.PIN; otp = inputSignatureAdvanced.OTP; credentialsID = inputSignatureAdvanced.credentialsID; access_token = accessToken; baseURL = base_URL; hashAlgo = inputSignatureAdvanced.hashAlgo; signAlgo = inputSignatureAdvanced.signAlgo;

            OutputError error = new OutputError()
            {
                error = "error_pades_signature",
                error_description = "error"

            };
            TElPDFDocument document = new TElPDFDocument();
            try
            {
                var filePath = Path.GetTempFileName();
                if (inputSignatureAdvanced.documentStream.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Stream memoryStream = new MemoryStream(inputSignatureAdvanced.documentStream);

                        await memoryStream.CopyToAsync(stream);
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
                            if (hashAlgo == "2.16.840.1.101.3.4.2.1")
                            {
                                handler.HashAlgorithm = SBConstants.__Global.SB_ALGORITHM_DGST_SHA256;
                            }
                            else
                            {
                                handler.HashAlgorithm = SBConstants.__Global.SB_ALGORITHM_DGST_SHA1;
                            }
                            handler.RemoteSigningMode = true;
                            handler.RemoteSigningCertIndex = 0;
                            handler.SignatureType = TSBPDFPublicKeySignatureType.pstPKCS7SHA1;
                            handler.SignatureSizeEstimationStrategy = TSBPAdESSignatureSizeEstimationStrategy.psesSmart;
                            handler.OnRemoteSign += new TSBPDFRemoteSignEvent(PAdESHandler_OnRemoteSign);

                            signature.Handler = handler;

                            document.Close(true);

                            document.Dispose();
                            stream.Close();
                            stream.Dispose();

                            memoryStream.Close();
                            memoryStream.Dispose();


                            var signedMemory = new MemoryStream();
                            using (FileStream signedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                await signedStream.CopyToAsync(signedMemory);

                                byte[] result = signedMemory.GetBuffer();

                                OutputAdvancedSignature output = new OutputAdvancedSignature()
                                {
                                    signedDocument = result
                                };

                                return output;
                            }
                        }

                        catch (Exception ex) { return error; }
                    }
                }
                else
                {
                    return error;
                }
            }
            catch (Exception ex)
            {
                return error;
            }

        }

        private TElMemoryCertStorage LoadCertificate(string credentialsID, string access_token)
        {

            //credentialsInfo
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();


            InputCredentialsInfo credentialsInfo = new InputCredentialsInfo() { credentialID = credentialsID };
            CredentialsInfoClient credInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);

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

            InputCredentialsAuthorize inputCredentialsAuthorize = new InputCredentialsAuthorize()
            {
                credentialID = credentialsID,
                hash = hashToSign,
                numSignatures = 1,
                OTP = otp,
                PIN = pin
            };

            CredentialsAuthorizeClient credentialsAuthorizeClient = new CredentialsAuthorizeClient(serializer, errorLogger, baseURL);
            string response = serializer.Serialize(credentialsAuthorizeClient.GetCredentialsAuthorize(access_token, inputCredentialsAuthorize));
            if (response != null && !response.Contains("error"))
            {
                OutputCredentialsAuthorize outCredAuth = serializer.Deserialize<OutputCredentialsAuthorize>(response);

                InputSignaturesSignHash inputSignatures = new InputSignaturesSignHash()
                {
                    credentialID = credentialsID,
                    hash = hashToSign,
                    hashAlgo = hashAlgo,
                    SAD = outCredAuth.SAD,
                    signAlgo = signAlgo

                };

                SignHashClient signHashClient = new SignHashClient(serializer, errorLogger, baseURL);
                string signResponse = serializer.Serialize(signHashClient.GetSignedHash(access_token, inputSignatures));
                if (!signResponse.Contains("error"))
                {
                    var signature = serializer.Deserialize<OutputSignaturesSignHash>(signResponse);
                    var signatureResult = signature.signatures.FirstOrDefault();
                    SignedHash = Encoding.UTF8.GetBytes(signatureResult);
                }




            }
        }
    }
}
