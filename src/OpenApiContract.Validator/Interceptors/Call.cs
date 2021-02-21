using System.Net.Http;

namespace OpenApiContract.Validator.Interceptors
{
    public class Call
    {
        public string Key { get; set; }
        public HttpRequestMessage HttpRequest { get; set; }
        public HttpResponseMessage HttpResponse { get; set; }
    }
}