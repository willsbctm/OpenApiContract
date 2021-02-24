using System.Net.Http;
using OpenApiContract.Validator.Assertions;
using OpenApiContract.Validator.Interceptors;

namespace OpenApiContract.Validator
{
    public static class FluentAssertionsExtensions
    {
        public static HttpRequestMessageAssertions Should(this HttpRequestMessage httpRequestMessage) =>
            new HttpRequestMessageAssertions(httpRequestMessage);

        public static HttpResponseMessageAssertions Should(this HttpResponseMessage httpResponseMessage) =>
            new HttpResponseMessageAssertions(httpResponseMessage);

        public static CallAssertions Should(this Call call) =>
            new CallAssertions(call);
    }
}
