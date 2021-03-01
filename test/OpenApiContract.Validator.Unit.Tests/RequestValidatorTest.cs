using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace OpenApiContract.Validator.Unit.Tests
{
    public class RequestValidatorTest
    {
        [Test]
        public void Validate_WhenValidateContentTypeWithVersionSpecification_ShouldBeValidated()
        {
            const string pathTemplate = "/path";
            
            var requestValidator = new RequestValidator(new[] {new JsonContentValidator()});

            var openApi = new OpenApiDocumentBuilder()
                .WithOperation(OperationType.Get)
                .WithPath(pathTemplate)
                .WithContentType("application/json; v=1")
                .Build();

            var stringContent = new StringContent("{ A: 'B' }");
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            stringContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("v", "1"));

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, pathTemplate)
            {
                Content = stringContent
            };

            Action validate = () => requestValidator.Validate(
                httpRequestMessage,
                openApi,
                pathTemplate);

            validate.Should().NotThrow();
        }
    }
}