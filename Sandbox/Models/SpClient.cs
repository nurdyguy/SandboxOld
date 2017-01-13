using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sandbox.Models
{
    public interface ISpClient
    {
        Task<ApiResponse<T>> GetResult<T>(string url, int endpointVersion = 1);
        Task<ApiResponse<T>> PostResult<T>(string url, object bodyData, int endpointVersion = 1);
        Task<ApiResponse<T>> PutResult<T>(string url, object bodyData, int endpointVersion = 1);
        Task<ApiResponse<T>> DeleteResult<T>(string url, object bodyData, int endpointVersion = 1);
    }
    public class SpClient : BaseApiClient, ISpClient
    {
        protected override string baseUrl
        {
            get { return "https://api.spotify.com"; }
            set { }
        }        
    }
}