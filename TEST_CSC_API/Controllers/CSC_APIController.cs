using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TEST_CSC_API.Controllers;

namespace TEST_CSC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSC_APIController : ControllerBase
    {
        IConfiguration _configuration;
        IAccessToken _accessToken;

        public CSC_APIController(IConfiguration configuration, IAccessToken accessToken)
        {
            _configuration = configuration;
            _accessToken = accessToken;
        }

        //ar trebui ca requesturile sa fie ok ca primeste parametrii diferiti

        [HttpGet]
        [Route("auth")]
        public object Auth()
        {
            return _accessToken.GetAccessToken();
        }

        [HttpPost]
        [Route("credentials")]
        public object Credentials(InputCredentialsList inputCredentialsList)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsListClient credentialsListClient = new CredentialsListClient(serializer, errorLogger, baseURL);
            Microsoft.Extensions.Primitives.StringValues value;
            string access_token = "";
            if (Request.Headers.TryGetValue("Authorization", out value))
            {
                access_token = value.ToString().Replace("Bearer ", "");
            }
            else
            {
                OutputError error = new OutputError()
                {
                    error = "invalid_access_token",
                    error_description = "Invalid access_token"
                };
                return serializer.Serialize(error);
            }
            object response = credentialsListClient.GetCredentialsList(access_token, inputCredentialsList);
            var credList = serializer.Serialize(response);
            OutputCredentialsList outputCredentialsList = serializer.Deserialize<OutputCredentialsList>(credList);
            //List<OutputCredentialsInfo> userCredInfo = new List<OutputCredentialsInfo>();
            List<OutputCredentials> userCredInfo = new List<OutputCredentials>();

            if (outputCredentialsList != null && outputCredentialsList.credentialIDs != null)
            {

                foreach (var credID in outputCredentialsList.credentialIDs)
                {
                    InputCredentialsInfo inputCredentialsInfo = new InputCredentialsInfo()
                    {
                        credentialID = credID
                    };

                    CredentialsInfoClient credentialsInfoClient = new CredentialsInfoClient(serializer, errorLogger, baseURL);
                    var credInfoResponse = serializer.Serialize(credentialsInfoClient.GetCredentialsInfo(access_token, inputCredentialsInfo));
                    if (!credInfoResponse.Contains("error"))
                    {
                        OutputCredentialsInfo outputCredentialsInfo = serializer.Deserialize<OutputCredentialsInfo>(credInfoResponse);
                        OutputCredentials tempOutputCred = new OutputCredentials();
                        tempOutputCred.credentialID = credID;
                        tempOutputCred.authMode = outputCredentialsInfo.authMode;
                        tempOutputCred.cert = outputCredentialsInfo.cert;
                        tempOutputCred.description = outputCredentialsInfo.description;
                        tempOutputCred.key = outputCredentialsInfo.key;
                        tempOutputCred.lang = outputCredentialsInfo.lang;
                        tempOutputCred.multisign = outputCredentialsInfo.multisign;
                        tempOutputCred.OTP = outputCredentialsInfo.OTP;
                        tempOutputCred.PIN = outputCredentialsInfo.PIN;
                        tempOutputCred.SCAL = outputCredentialsInfo.SCAL;

                        userCredInfo.Add(tempOutputCred);
                    }

                }
            }
            //pentru fiecare id din lista de credentiale sa returnez tot certificatul
            return userCredInfo;

        }

        [HttpPost]
        [Route("otp")]
        public object OTP(InputCredentialsSendOTP sendOTP)
        {
            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsSendOTPClient sendOTPClient = new CredentialsSendOTPClient(serializer, errorLogger, baseURL);

            Microsoft.Extensions.Primitives.StringValues value;
            string access_token = "";
            if (Request.Headers.TryGetValue("Authorization", out value))
            {
                access_token = value.ToString().Replace("Bearer ", "");
            }
            else
            {
                OutputError error = new OutputError()
                {
                    error = "invalid_access_token",
                    error_description = "Invalid access_token"
                };
                return serializer.Serialize(error);
            }


            object response = sendOTPClient.GetCredentialsSendOTP(access_token, sendOTP);

            return response;
        }


        [HttpPost]
        [Route("sign")]
        public object Sign(InputAuthorizeSignHash inputAuthSign)
        {
            InputCredentialsAuthorize inputCredentialsAuthorize = new InputCredentialsAuthorize()
            {
                credentialID = inputAuthSign.credentialsID,
                numSignatures = inputAuthSign.numSignatures,
                hash = inputAuthSign.hash,
                PIN = inputAuthSign.PIN,
                OTP = inputAuthSign.OTP,
                clientData = inputAuthSign.clientData,
                description = inputAuthSign.description
            };

            JsonSerializer serializer = new JsonSerializer();
            ErrorLogger errorLogger = new ErrorLogger();
            string baseURL = _configuration.GetSection("Transsped").GetSection("BaseURL").Value;

            CredentialsAuthorizeClient credentialsAuthorizeClient = new CredentialsAuthorizeClient(serializer, errorLogger, baseURL);

            Microsoft.Extensions.Primitives.StringValues value;
            string access_token = "";
            if (Request.Headers.TryGetValue("Authorization", out value))
            {
                access_token = value.ToString().Replace("Bearer ", "");
            }
            else
            {
                OutputError error = new OutputError()
                {
                    error = "invalid_access_token",
                    error_description = "Invalid access_token"
                };
                return serializer.Serialize(error);
            }

            string response = serializer.Serialize(credentialsAuthorizeClient.GetCredentialsAuthorize(access_token, inputCredentialsAuthorize));

            if (response != null && !response.Contains("error"))
            {
                OutputCredentialsAuthorize outCredAuth = serializer.Deserialize<OutputCredentialsAuthorize>(response);

                InputSignaturesSignHash inputSignatures = new InputSignaturesSignHash()
                {
                    clientData = inputAuthSign.clientData,
                    credentialID = inputAuthSign.credentialsID,
                    hash = inputAuthSign.hash,
                    hashAlgo = inputAuthSign.hashAlgo,
                    SAD = outCredAuth.SAD,
                    signAlgo = inputAuthSign.signAlgo,
                    signAlgoParams = inputAuthSign.signAlgoParams
                };

                SignHashClient signHashClient = new SignHashClient(serializer, errorLogger, baseURL);
                //string signResponse = serializer.Serialize(signHashClient.GetSignedHash(access_token, inputSignatures));


                return signHashClient.GetSignedHash(access_token, inputSignatures);
            }
            else
            {
                OutputError error = new OutputError()
                {
                    error = "invalid_access_token",
                    error_description = "Invalid access_token"
                };
                return error;
            }

        }


        //credentialsID, sadm hash, hashalgo, signalgo, signalgoparams, clientdata, numsignatures, pin, otp, description

    }
}