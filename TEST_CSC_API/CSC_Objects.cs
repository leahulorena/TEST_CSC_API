using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TEST_CSC_API
{
    //Contains information about the signature certificate
    [DataContract]
    public class Cert
    {
        //status schema - valid|expired|revoked|suspended    
        [DataMember]
        public string status { get; set; }

        //Contains one or more Base64-encoded X.509v3 certificates from the certificate chain
        [DataMember]
        public string[] certificates { get; set; }

        //The Issuer Subject Distinguished Name from the X.509v3 end entity certificate in printable string format
        [DataMember]
        public string issuerDN { get; set; }

        //The Serial Number from the X.509v3 certificate in hex encoded format
        [DataMember]
        public string serialNumber { get; set; }

        //The Distinguished Name from the X.509v3 certificate in printable string format
        [DataMember]
        public string subjectDN { get; set; }

        //The validity start date from the X.509v3 certificate in printable string format
        [DataMember]
        public string validFrom { get; set; }

        //The validity end date from the X.509v3 certificate in printable string format
        [DataMember]
        public string validTo { get; set; }

    }

    [DataContract]
    public class Key
    {
        //The status of enablement of the signing key of the credential
        [DataMember]
        public string status { get; set; }

        //The list of OIDs of the supported key algorithms
        [DataMember]
        public string[] algo { get; set; }

        //The length of the cryptographic key in bits
        [DataMember]
        public int len { get; set; }

        //The OID of the ECDSA curve
        [DataMember]
        public int curve { get; set; }

    }


    [DataContract]
    public class OTPINFO
    {
        //Specifies if a text-based OTP is required or not, or optional
        [DataMember]
        public string presence { get; set; }

        //Specifies if a text-based OTP is required or not, or optional
        [DataMember]
        public string type { get; set; }

        //Specifies the data format of the OTP
        [DataMember]
        public string format { get; set; }

        //Specifies an optional label for the data field used to collect the OTP in the user interface
        [DataMember]
        public string label { get; set; }

        //Optionally specifies a free form description of the OTP mechanism in the language specified in the lang parameter
        [DataMember]
        public string description { get; set; }

        //Specifies the identifier of the OTP device or application
        [DataMember]
        public string ID { get; set; }

        // Optionally specifies the provider of the OTP device or application
        [DataMember]
        public string provider { get; set; }

    }

    [DataContract]
    public class PININFO
    {
        //Specifies if a text-based PIN is required or not, or optional
        [DataMember]
        public string presence { get; set; }

        //Specifies the format of the PIN
        [DataMember]
        public string format { get; set; }

        //The Label Schema
        [DataMember]
        public string label { get; set; }

        //It optionally specifies a free form description of the PIN in the language specified in the lang parameter
        [DataMember]
        public string description { get; set; }
    }

    //Input parameters for info method
    [DataContract]
    public class InputInfo
    {
        //Request a preferred language according to RFC 3066
        [DataMember]
        public string lang { get; set; }
    }

    //Input parameters for auth/login methos
    [DataContract]
    public class InputAuthLogin
    {
        //The long-lived refresh token returned from the previous HTTP Basic Authentication
        [DataMember]
        public string refresh_token { get; set; }

        //option that the user may activate during the authentication phase to stay signed in
        //required
        [DataMember]
        public bool rememberMe { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }



    //Input parameter for auth/revoke method
    [DataContract]
    public class InputAuthRevoke
    {
        //The token that the Signature Application wants to get revoked.
        //required
        [DataMember]
        public string token { get; set; }

        //Specifies an optional hint about the type of the token submitted for revocation.
        [DataMember]
        public string token_type_hint { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //Input parameter for oauth2/token methos
    [DataContract]
    public class InputOAuth2Token
    {
        //The grant type, which depends on the type of OAuth 2.0 flow
        //(authorization_code|client_credentials|refresh_token)
        //required
        [DataMember]
        public string grant_type { get; set; }

        //The authorization code returned by the authorization server
        [DataMember]
        public string code { get; set; }

        //The long-lived refresh token returned from the previous HTTP Basic Authentication
        [DataMember]
        public string refresh_token { get; set; }

        //This is the unique client ID previously assigned to the Signature Application by the Remote Service
        //required
        [DataMember]
        public string client_id { get; set; }

        //This is the client secret previously assigned to the Signature Application by the Remote Service
        //required
        [DataMember]
        public string client_secret { get; set; }

        //The URL where the user was redirected after the authorization process completed
        [DataMember]
        public string redirect_uri { get; set; }
    }


    //Input parameters for credentials/list method
    [DataContract]
    public class InputCredentialsList
    {
        //The user identifier associated to the user identity
        [DataMember]
        public string userID { get; set; }

        //Maximum number of items to return
        [DataMember]
        public string maxResults { get; set; }

        //The page token for the new page of items
        [DataMember]
        public string pageToken { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //Input parameters for credentials/info method
    [DataContract]
    public class InputCredentialsInfo
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //The Certificates Schema
        //none|single|chain
        [DataMember]
        public string certificates { get; set; }

        //Specifies if the information on the end entity certificate shall be returned as printable strings
        [DataMember]
        public bool certInfo { get; set; }

        //Specifies if the information on the authorization mechanisms supported by this credential (PIN and OTP groups) shall be returned
        [DataMember]
        public bool authInfo { get; set; }

        //Request a preferred language according to RFC 3066
        [DataMember]
        public string lang { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //Input parameters for credentials/authorize method
    [DataContract]
    public class InputCredentialsAuthorize
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //The number of signatures to authorize
        //required
        [DataMember]
        public int numSignatures { get; set; }

        //One or more Base64-encoded hash values to be signed
        [DataMember]
        public string[] hash { get; set; }

        //The PIN collected from the user
        [DataMember]
        public string PIN { get; set; }

        //The OTP collected from the user
        [DataMember]
        public string OTP { get; set; }

        //Contains a free form description of the authorization transaction in the lang language
        [DataMember]
        public string description { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }


    //Input parameters for credentials/extendTransaction method
    [DataContract]
    public class InputCredentialsExtendTransaction
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //#/components/schemas/hash"
        [DataMember]
        public string hash { get; set; }

        //The Signature Activation Data to provide as input to the signatures/signHash method.
        //required
        [DataMember]
        public string SAD { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //The Root Schema
    [DataContract]
    public class InputCredentialsSendOTP
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //Input parameters for signatures/signHash method
    [DataContract]
    public class InputSignaturesSignHash
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //The Signature Activation Data to provide as input to the signatures/signHash method.
        //required
        [DataMember]
        public string SAD { get; set; }


        //One or more Base64-encoded hash values to be signed
        [DataMember]
        public string[] hash { get; set; }

        //"Specifies the OID of the algorithm used to calculate the hash value(s)
        [DataMember]
        public string hashAlgo { get; set; }

        //Specifies the OID of the algorithm to use for signing
        [DataMember]
        public string signAlgo { get; set; }

        //Specifies the Base64-encoded of DER-encoded ASN.1 signature parameters
        [DataMember]
        public string signAlgoParams { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }

    //Input parameters for signature/timestamp method
    [DataContract]
    public class InputSignaturesTimestamp
    {
        //One or more Base64-encoded hash values to be signed
        [DataMember]
        public string[] hash { get; set; }

        //"Specifies the OID of the algorithm used to calculate the hash value(s)
        [DataMember]
        public string hashAlgo { get; set; }

        //Specifies a large random number with a high probability that it is generated by the Signature Application only once
        [DataMember]
        public string nonce { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }




    /*--------------------------------------------Objects for advanced signatures------------------------------------*/
    [DataContract]
    public class InputSignaturesSignPDF
    {
        //The identifier associated to the credential
        //required
        [DataMember]
        public string credentialID { get; set; }

        //The Signature Activation Data to provide as input to the signatures/signHash method.
        //required
        [DataMember]
        public string SAD { get; set; }


        //One or more Base64-encoded hash values to be signed
        [DataMember]
        public string[] hash { get; set; }

        //"Specifies the OID of the algorithm used to calculate the hash value(s)
        [DataMember]
        public string hashAlgo { get; set; }

        //Specifies the OID of the algorithm to use for signing
        [DataMember]
        public string signAlgo { get; set; }

        //Specifies the Base64-encoded of DER-encoded ASN.1 signature parameters
        [DataMember]
        public string signAlgoParams { get; set; }

        //Arbitrary data from the Signature Application
        [DataMember]
        public string clientData { get; set; }
    }


    [DataContract]
    public class OutputSignaturesSignPDF
    {
        //Array of Base64-encoded signed hash
        //required
        [DataMember]
        public string[] signatures { get; set; }
    }

    /*----------------------------------------------------------------------------------*/
    //Output parameters for info method
    [DataContract]
    public class OutputInfo
    {
        //The version of this specification implemented by the provider
        //required
        [DataMember]
        public string specs { get; set; }

        //The commercial name of the Remote Service
        //required
        [DataMember]
        public string name { get; set; }

        //The URI of the image file containing the logo of the Remote Service which shall be published online
        //required
        [DataMember]
        public string logo { get; set; }

        //The ISO 3166-1 Alpha-2 code of the Country where the Remote Service provider is established
        //required
        [DataMember]
        public string region { get; set; }

        //Request a preferred language according to RFC 3066
        //required
        [DataMember]
        public string lang { get; set; }

        //Specifies one or more values corresponding to the authentication mechanisms supported by the Remote Service
        //external|TLS|basic|digest|oauth2code|oauth2implicit|oauth2client
        //required
        [DataMember]
        public string[] authType { get; set; }

        //Specifies the complete URI of the OAuth 2.0 service authorization endpoint provided by the Remote Service
        //required
        [DataMember]
        public string oauth2 { get; set; }

        //Specifies the list of names of all the API methods
        //required
        [DataMember]
        public string[] methods { get; set; }
    }

    //Output parameters for auth/login method
    [DataContract]
    public class OutputAuthLogin
    {
        //The short-lived service access token used to authenticate the subsequent API requests within the same session.
        //required
        [DataMember]
        public string access_token { get; set; }

        //The long-lived refresh token returned from the previous HTTP Basic Authentication
        [DataMember]
        public string refresh_token { get; set; }

        //The lifetime in seconds of the service access token
        [DataMember]
        public int expires_in { get; set; }
    }

    //Output parameters for oauth2/token method
    [DataContract]
    public class OutputOauth2Token
    {
        //The short-lived service access token used to authenticate the subsequent API requests within the same session.
        //required
        [DataMember]
        public string access_token { get; set; }

        //The long-lived refresh token returned from the previous HTTP Basic Authentication
        [DataMember]
        public string refresh_token { get; set; }

        //Specifies a Bearer token type as defined in RFC6750
        //required
        [DataMember]
        public string token_type { get; set; }

        //The lifetime in seconds of the service access token
        [DataMember]
        public int expires_in { get; set; }
    }

    //Output parameters for credentials/list method
    [DataContract]
    public class OutputCredentialsList
    {
        //One or more credentialID associated with the provided or implicit userID
        //required
        [DataMember]
        public string[] credentialIDs { get; set; }

        //The page token for the next page of items
        [DataMember]
        public string nextPageToken { get; set; }
    }

    //Output parameters for credentials/info method
    [DataContract]
    public class OutputCredentialsInfo
    {
        //Contains a free form description of the authorization transaction in the lang language
        [DataMember]
        public string description { get; set; }

        //Information about the key
        //required
        [DataMember]
        public Key key { get; set; }

        //Contains information about the signature certificate
        //required
        [DataMember]
        public Cert cert { get; set; }

        //Specifies one of the authorization modes 
        //implicit|explicit|oauth2code|oauth2token
        //required
        [DataMember]
        public string authMode { get; set; }

        //Specifies the Sole Control Assurance Level required by the credential, as defined in CEN EN 419 241-1
        //required
        [DataMember]
        public string SCAL { get; set; }

        //Contains information about the credential's PIN
        //required
        [DataMember]
        public PININFO PIN { get; set; }

        //Contains information about the credential's OTP
        //required
        [DataMember]
        public OTPINFO OTP { get; set; }

        //Specifies if the credential supports multiple signatures to be created with a single authorization request
        //required
        [DataMember]
        public bool multisign { get; set; }

        //Request a preferred language according to RFC 3066
        //required
        [DataMember]
        public string lang { get; set; }
    }

    //Output parameters for credentials/authorize method
    [DataContract]
    public class OutputCredentialsAuthorize
    {
        //
        //required
        [DataMember]
        public string SAD { get; set; }

        //
        [DataMember]
        public string expired_In { get; set; }
    }

    //Output parameters for credentials/authorize method
    [DataContract]
    public class OutputCredentialsExtendTransaction
    {

        //The Signature Activation Data to provide as input to the signatures/signHash method.
        //required
        [DataMember]
        public string SAD { get; set; }

        //"#/components/schemas/expiresIn
        [DataMember]
        public string expired_In { get; set; }
    }

    //Otput parameters for signatures /signHah
    [DataContract]
    public class OutputSignaturesSignHash
    {
        //Array of Base64-encoded signed hash
        //required
        [DataMember]
        public string[] signatures { get; set; }
    }

    //Output parameters for signatures/timestamp method
    [DataContract]
    public class OutputSinaturesTimestamp
    {
        //The Base64-encoded time-stamp token as defined in RFC 3161 as updated by RFC 5816
        //required
        [DataMember]
        public string timestamp { get; set; }
    }

    [DataContract]
    public class OutputError
    {
        [DataMember]
        public string error { get; set; }

        [DataMember]
        public string error_description { get; set; }
    }
}
