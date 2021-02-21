using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenApiContract.Validator.Api;
using System.Net.Http;

namespace OpenApiContract.Validator.Integration.Tests
{
    [SetUpFixture]
    public class Setup
    {
        public static HttpClient MeuClient { get; set; }

        public static WebApplicationFactory<Program> Factory { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetupUp()
        {
            MeuClient = new HttpClient();
            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddTransient(httpClient =>
                    {
                        var httpClientFactory = A.Fake<IHttpClientFactory>();
                        A.CallTo(() => httpClientFactory.CreateClient("pet")).Returns(MeuClient);
                        return httpClientFactory;
                    });
                });
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Factory = new WebApplicationFactory<Program>();
        }
    }
}
