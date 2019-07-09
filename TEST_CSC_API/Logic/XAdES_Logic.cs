using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBCustomCertStorage;
using SBX509;
using SBXMLAdES;
using SBXMLAdESIntf;
using SBXMLCore;
using SBXMLSec;
using SBXMLSig;
using SBXMLTransform;
using TEST_CSC_API.Controllers;

namespace TEST_CSC_API.Logic
{
    public class XAdES_Logic
    {

        private string pin;
        private string otp;
        private string access_token;
        private string credentialsID;
        private string baseURL;
        private string hashAlgo;
        private string signAlgo;

        public async Task<object> SignXMLAsync(string accessToken, string base_URL, InputSignatureAdvanced inputSignatureAdvanced)
        {

            SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");
            pin = inputSignatureAdvanced.PIN; otp = inputSignatureAdvanced.OTP; credentialsID = inputSignatureAdvanced.credentialsID; access_token = accessToken; baseURL = base_URL; hashAlgo = inputSignatureAdvanced.hashAlgo; signAlgo = inputSignatureAdvanced.signAlgo;

            OutputError error = new OutputError()
            {
                error = "error_pades_signature",
                error_description = "error"

            };

            TElXMLDOMDocument document = new TElXMLDOMDocument();
            TElXMLDOMDocument signedDocument = new TElXMLDOMDocument();

            try
            {
                var filePath = Path.GetTempFileName();
                if (inputSignatureAdvanced.documentStream.Length > 0)
                {
                    using (Stream stream = new FileStream(filePath, FileMode.Create))
                    {
                        Stream memoryStream = new MemoryStream(inputSignatureAdvanced.documentStream);

                        await memoryStream.CopyToAsync(stream);
                        stream.Position = 0;
                        document.LoadFromStream(stream, "ISO-8859-1", true);

                        TElXMLSigner Signer = new TElXMLSigner(null);
                        TElXMLKeyInfoX509Data X509Data = new TElXMLKeyInfoX509Data(false);
                        try
                        {
                            Signer.SignatureType = SBXMLSec.Unit.xstEnveloped;
                            Signer.CanonicalizationMethod = SBXMLDefs.Unit.xcmCanon;
                            Signer.SignatureMethodType = SBXMLSec.Unit.xmtSig;

                            TElXMLReference Ref = new TElXMLReference();

                            Ref.URI = "";
                            Ref.URINode = document.DocumentElement;
                            Ref.TransformChain.AddEnvelopedSignatureTransform();

                            if (hashAlgo == "2.16.840.1.101.3.4.2.1")
                            {

                                Signer.SignatureMethod = SBXMLSec.Unit.xsmRSA_SHA256;
                                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                            }
                            else
                            {

                                Signer.SignatureMethod = SBXMLSec.Unit.xsmRSA_SHA1;
                                Ref.DigestMethod = SBXMLSec.Unit.xdmSHA1;
                            }

                            Signer.References.Add(Ref);

                            TElX509Certificate Cert = LoadCertificate(credentialsID, accessToken);
                            X509Data.Certificate = Cert;
                            Signer.KeyData = X509Data;

                            Signer.UpdateReferencesDigest();
                            Signer.OnRemoteSign += new TSBXMLRemoteSignEvent(XAdESHandler_OnRemoteSign);
                            Signer.GenerateSignature();
                            TElXMLDOMNode node = document.ChildNodes.get_Item(0);

                            Signer.SaveEnveloped(document.DocumentElement);
                            var signedMemory = new MemoryStream();
                            document.SaveToStream(signedMemory);

                            OutputAdvancedSignature output = new OutputAdvancedSignature()
                            {
                                signedDocument = signedMemory.GetBuffer()
                            };

                            Signer.Dispose();
                            X509Data.Dispose();
                            return output;

                        }
                        catch (Exception ex)
                        {
                            return error;
                        }
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


        private void XAdESHandler_OnRemoteSign(object Sender, byte[] Hash, ref byte[] SignedHash)
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
