using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Sandbox.Models
{
    public class ApiTestModel
    {
        public List<Track> tracks { get; set; }
    }
    
    public class Track
    {
        public string uri { get; set; }
    }

    public class ApiResponse<T>
    {
        public T Response { get; set; }
        public virtual bool IsSuccessful
        {
            get { return this.Error == null; }
        }
        public Exception Error { get; set; }
        public virtual HttpStatusCode ResponseCode { get; set; }
    }
    
}