using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using FakeItEasy;
using System.Net;
using OpenApiContract.Validator.Interceptors;
using OpenApiContract.Validator.Api.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace OpenApiContract.Validator.Integration.Tests
{

    public class PetControllerTests : Base
    {
        private OpenApiDocument documentoOpenApi;

        [SetUp]
        public async Task OneTimeSetup()
        {
            using var httpClientOpenApi = new HttpClient();
            var openApiJson = await httpClientOpenApi.GetStringAsync($"https://petstore.swagger.io/v2/swagger.json");
            using var arquivo = new MemoryStream(Encoding.UTF8.GetBytes(openApiJson));
            documentoOpenApi = new OpenApiStreamReader().Read(arquivo, out _);
        }

        [Test]
        public async Task ShouldCallGetRequest()
        {
            var chave = Faker.Random.AlphaNumeric(10);
            var pet = new Pet
            {
                Name = Faker.Person.FirstName,
                PhotoUrls = new List<string>
                {
                    Faker.Internet.Url()
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(pet, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), Encoding.UTF8, "application/json");
            var handler = A.Fake<InterceptorFakeHandler>(x => x.CallsBaseMethods());
            A.CallTo(() => handler.FakeSend(A<HttpRequestMessage>._))
                .Returns(new InterceptedResponse
                {
                    Key = chave,
                    HttpResponse = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = content
                    }
                });
            Setup.MeuClient = new HttpClient(handler);

            var url = $"{host}/pet";

            _ = await httpClient.GetAsync(url);

            var call = handler.GetCall(chave);
            call.Should().SatisfyEspecification(documentoOpenApi, "/pet/{petId}", HttpStatusCode.OK);
        }

        [Test]
        public async Task ShouldCallPostRequest()
        {
            var chave = Faker.Random.AlphaNumeric(10);
            var pet = new Pet
            {
                Name = Faker.Person.FirstName,
                PhotoUrls = new List<string>
                {
                    Faker.Internet.Url()
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(pet, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), Encoding.UTF8, "application/json");
            
            var handler = A.Fake<InterceptorFakeHandler>(x => x.CallsBaseMethods());
            A.CallTo(() => handler.FakeSend(A<HttpRequestMessage>._))
                .Returns(new InterceptedResponse
                {
                    Key = chave,
                    HttpResponse = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.MethodNotAllowed,
                        Content = new StringContent(Faker.Random.Words(3), Encoding.UTF8, "application/json")
                    }
                });
            
            Setup.MeuClient = new HttpClient(handler);

            var url = $"{host}/pet";

            _ = await httpClient.PostAsync(url, content);

            var call = handler.GetCall(chave);
            call.Should().SatisfyEspecification(documentoOpenApi, "/pet", HttpStatusCode.MethodNotAllowed);
        }
    }
}