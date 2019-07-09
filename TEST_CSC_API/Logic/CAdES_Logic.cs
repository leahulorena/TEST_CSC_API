using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBCAdES;
using SBCertValidator;
using SBCMS;
using SBCustomCertStorage;
using SBX509;
using TEST_CSC_API.Controllers;

namespace TEST_CSC_API.Logic
{

    public class CAdES_Logic
    {
        private string pin;
        private string otp;
        private string access_token;
        private string credentialsID;
        private string baseURL;
        private string hashAlgo;
        private string signAlgo;

        public async Task<object> SignCMSAsync(string accessToken, string base_URL, InputSignatureAdvanced inputSignatureAdvanced)
        {
            SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");
            pin = inputSignatureAdvanced.PIN; otp = inputSignatureAdvanced.OTP; credentialsID = inputSignatureAdvanced.credentialsID; access_token = accessToken; baseURL = base_URL; hashAlgo = inputSignatureAdvanced.hashAlgo; signAlgo = inputSignatureAdvanced.signAlgo;

            OutputError error = new OutputError()
            {
                error = "error_pades_signature",
                error_description = "error"

            };

            var filePath = Path.GetTempFileName();
            if (inputSignatureAdvanced.documentStream.Length > 0)
            {
                using (Stream stream = new FileStream(filePath, FileMode.Create))
                {
                    Stream memoryStream = new MemoryStream(inputSignatureAdvanced.documentStream);

                    await memoryStream.CopyToAsync(stream);
                    var msg = new TElSignedCMSMessage();
                    msg.CreateNew(stream, 0, stream.Length);
                    int sigIndex = msg.AddSignature();

                    SBPKCS7Utils.TElPKCS7Attributes pKCS7Attributes = new SBPKCS7Utils.TElPKCS7Attributes();

                    TElCMSSignature signature = msg.get_Signatures(sigIndex);

                    TElX509Certificate certificate = LoadCertificate(credentialsID, access_token);

                    if (hashAlgo == "2.16.840.1.101.3.4.2.1")
                    {
                        signature.DigestAlgorithm = SBConstants.Unit.SB_ALGORITHM_DGST_SHA256;
                    }
                    else
                    {
                        signature.DigestAlgorithm = SBConstants.Unit.SB_ALGORITHM_DGST_SHA1;
                    }

                    signature.SigningOptions = SBCMS.__Global.csoInsertMessageDigests |
                                               SBCMS.__Global.csoIncludeCertToAttributes |
                                               SBCMS.__Global.csoIncludeCertToMessage |
                                               SBCMS.__Global.csoInsertContentType |
                                               SBCMS.__Global.csoInsertSigningTime |
                                               SBCMS.__Global.csoUsePlainContentForTimestampHashes;


                    signature.SigningTime = DateTime.Now;

                    int cID = signature.SigningCertificate.AddCertID();
                    TElCMSSignerIdentifier signerIdentifier = signature.SigningCertificate.get_CertIDs(cID);
                    signerIdentifier.Import(certificate, SBConstants.Unit.SB_ALGORITHM_DGST_SHA1);

                    signature.SigningCertificate.SigningCertificateType = TSBCMSSigningCertificateType.sctESSSigningCertificateV2;
                    signature.SigningCertificate.Included = true;


                    TElCAdESSignatureProcessor processor = new TElCAdESSignatureProcessor();
                    processor.RemoteSigningMode = true;
                    processor.AllowPartialValidationInfo = true;
                    processor.ForceCompleteChainValidation = false;
                    processor.ForceSigningCertificateV2 = false;
                    processor.IgnoreChainValidationErrors = true;
                    processor.OfflineMode = false;
                    processor.SkipValidationTimestampedSignatures = true;


                    processor.Signature = signature;
                    processor.OnRemoteSign += new TSBCAdESRemoteSignEvent(CAdES_Handler);
                    processor.CreateBES(certificate);



                    var result = new MemoryStream();
                    msg.Save(result);

                    OutputAdvancedSignature output = new OutputAdvancedSignature()
                    {
                        signedDocument = result.GetBuffer()
                    };
                    return output;

                }
            }
            else
            {
                return error;
            }
        }

        private TElX509Certificate LoadCertificate(string credentialsID, string access_token)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            CredentialsInfoClient credInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);

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

                return cert;
            }
            else return null;
        }

        private void CAdES_Handler(object Sender, byte[] Hash, ref byte[] SignedHash)
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
