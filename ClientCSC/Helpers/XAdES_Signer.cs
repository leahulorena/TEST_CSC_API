using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientCSC.Models;
using SBCustomCertStorage;
using SBX509;
using SBXMLAdES;
using SBXMLAdESIntf;
using SBXMLCore;
using SBXMLSec;
using SBXMLSig;
using SBXMLTransform;

namespace ClientCSC.Helpers
{
    public class XAdES_Signer
    {
        private string pin;
        private string otp;
        private string access_token;
        private string credentialsID;
        private string baseURL;
        private string hashAlgo;
        private string signAlgo;


        public MemoryStream SignXML(Stream stream, string accessToken, string OTP, string PIN, string credentialID, string base_URL, string hash_algo, string sign_algo)
        {
            var memory = new MemoryStream();
            try
            {
                SBUtils.Unit.SetLicenseKey("03D250F599AFD170E8A7410AFE3EAAC635E687187762F9936518B7FA6AEDDB215DF3177560DD647433BEE43711D31EC2B6818C0797C464E7F077467EABB466DE8F21CE77A054C9D3B04B0BA859B4BE8E8B7FCD50D07E2A4CD96240FA1468D8F03CBDE4EB1D2070A4294D2426881EEFBDFFAA7A76747B30A2E0564CA06CD673089318BFBA530E88A26F6FF76E46FE2A5A65C0FBAACB09F9804BC287412E49EE832058643D8A59B8398C7637C3EDE91660E6B696F32AD416F606DB215A2FFF214B5DF58DE27687362740B591D7F3D2D33CE6A3D1601521408511476FA81D374CA32D0443BD710D4D732A8C398A953047EEAB4A62237813DA11FC5E0EBFF1E69A9D");

                pin = PIN; otp = OTP; credentialsID = credentialID; access_token = accessToken; baseURL = base_URL; hashAlgo = hash_algo; signAlgo = sign_algo;

                TElXMLDOMDocument document = new TElXMLDOMDocument();
                TElXMLDOMDocument signedDocument = new TElXMLDOMDocument();

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

                    TElX509Certificate Cert = LoadCertificate(credentialsID, access_token);
                    X509Data.Certificate = Cert;
                    Signer.KeyData = X509Data;

                    Signer.UpdateReferencesDigest();
                    Signer.OnRemoteSign += new TSBXMLRemoteSignEvent(XAdESHandler_OnRemoteSign);
                    Signer.GenerateSignature();
                    TElXMLDOMNode node = document.ChildNodes.get_Item(0);

                     Signer.SaveEnveloped(document.DocumentElement);
                  
                    // Signer.SaveEnveloping(node);
                   // Signer.SaveDetached(); - semnatura se salveaza separat

                  

                    document.SaveToStream(memory);

                    return memory;

                }
                finally
                {
                    Signer.Dispose();
                    X509Data.Dispose();
                }
            }
            catch (Exception ex) { return memory; }
        }

        private TElX509Certificate LoadCertificate(string credentialsID, string access_token)
        {
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

                return cert;
            }
            else return null;
        }

        private void XAdESHandler_OnRemoteSign(object Sender, byte[] Hash, ref byte[] SignedHash)
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

}
