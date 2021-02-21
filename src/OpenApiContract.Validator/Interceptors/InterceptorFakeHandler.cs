using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace OpenApiContract.Validator.Interceptors
{
    public abstract class InterceptorFakeHandler : HttpMessageHandler
    {
        private readonly List<Call> Calls;

        public InterceptorFakeHandler() => Calls = new List<Call>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = FakeSend(request);
            AddCallLog(response.Key, request, response.HttpResponse);
            return Task.FromResult(response.HttpResponse);
        }

        public abstract InterceptedResponse FakeSend(HttpRequestMessage request);

        private void AddCallLog(string key, HttpRequestMessage request, HttpResponseMessage response) =>
            Calls.Add(new Call { Key = key, HttpRequest = request, HttpResponse = response });

        public Call GetCall(string key) => Calls.SingleOrDefault(x => x.Key == key);
    }
}