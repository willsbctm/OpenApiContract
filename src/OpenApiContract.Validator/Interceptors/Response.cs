using System.Net.Http;

namespace OpenApiContract.Validator.Interceptors
{
    public class InterceptedResponse
    {
        public string Key { get; set; }
        public HttpResponseMessage HttpResponse { get; set; }
    }
}