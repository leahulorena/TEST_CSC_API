using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;

namespace TEST_CSC_API
{
    public class BaseClient : RestClient
    {
        protected IErrorLogger _errorLogger;

        public BaseClient(IDeserializer serializer, IErrorLogger errorLogger, string baseURL)
        {
            var NewtonsoftJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            _errorLogger = errorLogger;
            AddHandler("application/json", serializer);
            AddHandler("text/json", serializer);
            AddHandler("text/x-json", serializer);
            BaseUrl = new Uri(baseURL);
        }

        public override IRestResponse Execute(IRestRequest request)
        {
            var response = base.Execute(request);
            TimeoutCheck(request, response);
            return response;
        }

        public override IRestResponse<T> Execute<T>(IRestRequest request)
        {
            var response = base.Execute<T>(request);
            TimeoutCheck(request, response);
            return response;
        }

        public T Get<T>(IRestRequest request) where T : new()
        {
            var response = Execute<T>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                LogError(BaseUrl, request, response);
                return default(T);
            }
        }

        public void LogError(Uri baseURL, IRestRequest request, IRestResponse response)
        {
            string parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + ((x.Value == null) ? "NULL" : x.Value)).ToArray());

            string info = "Request to " + baseURL.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ",  parameters: " + parameters + ",  and content: " + response.Content;

            Exception ex;

            if (response != null && response.ErrorException != null)
            {
                ex = response.ErrorException;
            }
            else
            {
                ex = new Exception(info);
                info = String.Empty;
            }

            _errorLogger.LogError(ex, info);
        }

        private void TimeoutCheck(IRestRequest request, IRestResponse response)
        {
            if (response.StatusCode == 0)
            {
                LogError(BaseUrl, request, response);
            }
        }
    }
}
