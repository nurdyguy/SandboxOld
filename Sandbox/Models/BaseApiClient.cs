//using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Sandbox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Models
{
    public abstract class BaseApiClient 
    {
        private readonly IList<JsonConverter> converters;

        protected abstract string baseUrl { get; set; }
        protected virtual byte[] key { get; set; }
        
        public BaseApiClient(IList<JsonConverter> converters = null)
        {
            this.converters = converters;            
        }

        public async Task<ApiResponse<T>> GetResult<T>(string url, int endpointVersion = 1)
        {
            return await HttpResult<T>(url, null, endpointVersion, HttpMethod.Get);
        }

        public async Task<ApiResponse<T>> PostResult<T>(string url, object bodyData, int endpointVersion = 1)
        {
            return await HttpResult<T>(url, bodyData, endpointVersion, HttpMethod.Post);
        }

        public async Task<ApiResponse<T>> PutResult<T>(string url, object bodyData, int endpointVersion = 1)
        {
            return await HttpResult<T>(url, bodyData, endpointVersion, HttpMethod.Put);
        }

        public async Task<ApiResponse<T>> DeleteResult<T>(string url, object bodyData, int endpointVersion = 1)
        {
            return await HttpResult<T>(url, bodyData, endpointVersion, HttpMethod.Delete);
        }

        private async Task<ApiResponse<T>> HttpResult<T>(string url, object bodyData, int endpointVersion, HttpMethod method)
        {
            ApiResponse<T> response = null;
            try
            {
                var fullUri = GetFullUrl(url);
                response = await MakeWebCall<T>(fullUri, bodyData, method, endpointVersion);
            }
            //catch (ApiException apiException)
            //{
            //    response = new ApiResponse<T>();
            //    response.ResponseCode = apiException.StatusCode;
            //    response.Error = apiException;
            //}
            catch (Exception ex)
            {
                response = new ApiResponse<T>();
                response.ResponseCode = HttpStatusCode.InternalServerError;
                response.Error = ex;
            }
            return response;
        }

        //protected async Task<ApiResponse<T>> UploadFile<T>(string url, string mediaType, HttpMethod method, Stream image, int endpointVersion = 1)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        string requestHash = "";
        //        using (HMACSHA256 hmac = new HMACSHA256(key))
        //        {
        //            string hashMe = url + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        //            var bytes = Encoding.ASCII.GetBytes(hashMe);
        //            byte[] hashValue = hmac.ComputeHash(bytes);
        //            requestHash = Convert.ToBase64String(hashValue);
        //        }

        //        var fullUri = GetFullUrl(url);

        //        using (var streamContent = new StreamContent(image))
        //        {
        //            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        //            var requestMessage = new HttpRequestMessage
        //            {
        //                Content = streamContent,
        //                RequestUri = new Uri(fullUri),
        //                Method = method
        //            };

        //            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue($"application/vnd.rnapi.v{endpointVersion}+json"));

        //            using (var response = await client.SendAsync(requestMessage))
        //            {
        //                var responseString = await response.Content.ReadAsStringAsync();
        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    throw new ApiException(response.StatusCode, responseString);
        //                }

        //                var jsonSettings = new JsonSerializerSettings
        //                {
        //                    Converters = converters ?? new List<JsonConverter>(),
        //                    NullValueHandling = NullValueHandling.Ignore
        //                };
        //                T responseObject = JsonConvert.DeserializeObject<T>(responseString, jsonSettings);

        //                var apiResponse = new ApiResponse<T>
        //                {
        //                    Response = responseObject,
        //                    ResponseCode = response.StatusCode
        //                };
        //                return apiResponse;
        //            }
        //        }
        //    }
        //}

        private async Task<ApiResponse<T>> MakeWebCall<T>(string url, object bodyData, HttpMethod method, int endpointVersion)
        {
            using (var client = new HttpClient())
            {
                //string hash = HashRequest(url, bodyData);
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", $"{Settings.ApiAppId}:{hash}");
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue($"application/vnd.rnapi.v{endpointVersion}+json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var jsonSettings = new JsonSerializerSettings
                {
                    Converters = converters ?? new List<JsonConverter>(),
                    NullValueHandling = NullValueHandling.Ignore
                };

                String json = String.Empty;
                if (method != HttpMethod.Get && bodyData != null)
                    json = JsonConvert.SerializeObject(bodyData, jsonSettings);

                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                using (var requestMessage = new HttpRequestMessage(method, url))
                {
                    if(method != HttpMethod.Get)
                        requestMessage.Content = stringContent;
                    AddHeadersToRequest(requestMessage);
                    try
                    {
                        using (var response = await client.SendAsync(requestMessage))
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            if (!response.IsSuccessStatusCode)
                            {
                                //throw new ApiException(response.StatusCode, responseString);
                                throw new Exception();
                            }

                            T responseObject = JsonConvert.DeserializeObject<T>(responseString, jsonSettings);
                            var apiResponse = new ApiResponse<T>
                            {
                                Response = responseObject,
                                ResponseCode = response.StatusCode
                            };
                            return apiResponse;
                        }
                    }
                    //catch (ApiException)
                    //{
                    //    throw;
                    //}
                    catch (Exception ex)
                    {
                        throw new Exception("Exception caught in HttpApiService.MakeWebCall. See inner exception for details.", ex);
                    }
                }
            }
        }

        private string GetFullUrl(string url)
        {
            StringBuilder fullUri = new StringBuilder();
            fullUri.Append(baseUrl);

            if (!url.StartsWith("/") && !baseUrl.EndsWith("/"))
                fullUri.Append("/");
            else if (url.StartsWith("/") && baseUrl.EndsWith("/"))
                url = url.Substring(1);

            fullUri.Append(url);
            return fullUri.ToString();
        }

        //private string HashRequest(string url, object bodyData)
        //{
        //    string hash = String.Empty;
        //    using (HMACSHA256 hmac = new HMACSHA256(key))
        //    {
        //        string hashMe = string.Empty;

        //        if (bodyData != null)
        //        {
        //            hashMe = JsonConvert.SerializeObject(bodyData);
        //        }
        //        else
        //        {
        //            hashMe = url + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        //        }

        //        var bytes = Encoding.ASCII.GetBytes(hashMe);
        //        byte[] hashValue = hmac.ComputeHash(bytes);
        //        hash = Convert.ToBase64String(hashValue);
        //    }
        //    return hash;
        //}

        private void AddHeadersToRequest(HttpRequestMessage request)
        {
        //    var userId = logDataRetriever.GetUserId();
        //    var orgId = logDataRetriever.GetOrgId();
        //    var staffId = logDataRetriever.GetStaffId();
        //    var sessionId = logDataRetriever.GetSessionId();
        //    var requestId = logDataRetriever.GetRequestId();

        //    request.Headers.Add("userId", userId?.ToString() ?? "");
        //    request.Headers.Add("orgId", orgId?.ToString() ?? "");
        //    request.Headers.Add("staffId", staffId?.ToString() ?? "");
        //    request.Headers.Add("sessionId", sessionId.ToString());
        //    request.Headers.Add("correlatedRequestId", requestId.ToString());
        }


        //using (var client = new HttpClient())
        //{
        //    client.BaseAddress = new Uri("https://api.spotify.com/");
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    var response = await client.GetAsync("v1/artists/671sBQXQM2vHSu0AGvpfDs/top-tracks?country=us");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseString = response.Content.ReadAsStringAsync().Result;
        //        var jsonSettings = new JsonSerializerSettings
        //        {
        //            Converters = new List<JsonConverter>(),
        //            NullValueHandling = NullValueHandling.Ignore
        //        };
        //        var model = JsonConvert.DeserializeObject<ApiTestModel>(responseString, jsonSettings);
        //    }
        //}
    }
}
