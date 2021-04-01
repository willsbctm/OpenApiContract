using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using NUnit.Framework;

namespace OpenApiContract.Validator.Unit.Tests
{
    public class RequestValidatorTest
    {
        [Test]
        public void ValidateContentType_WithVersionInBothSwaggerAndTheRequest_ShouldBeValidated()
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

        [Test]
        public void ValidateContentType_WithoutVersionInSwaggerButWithVersionInTheRequest_ShouldBeValidated()
        {
            const string pathTemplate = "/path";

            var requestValidator = new RequestValidator(new[] {new JsonContentValidator()});

            var openApi = new OpenApiDocumentBuilder()
                .WithOperation(OperationType.Get)
                .WithPath(pathTemplate)
                .WithContentType("application/json")
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

        [Test]
        public void ValidateContentType_WithoutEncodingInSwaggerButWithEncodingInTheRequest_ShouldBeValidated()
        {
            const string pathTemplate = "/path";

            var requestValidator = new RequestValidator(new[] {new JsonContentValidator()});

            var openApi = new OpenApiDocumentBuilder()
                .WithOperation(OperationType.Get)
                .WithPath(pathTemplate)
                .WithContentType("application/json")
                .Build();

            var stringContent = new StringContent("{ A: 'B' }", Encoding.UTF8, "application/json");

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

        [Test]
        public void ValidateContentType_WithEncodingInBothSwaggerAndRequest_ShouldBeValidated()
        {
            const string pathTemplate = "/path";

            var requestValidator = new RequestValidator(new[] {new JsonContentValidator()});

            var openApi = new OpenApiDocumentBuilder()
                .WithOperation(OperationType.Get)
                .WithPath(pathTemplate)
                .WithContentType("application/json; charset=utf-8")
                .Build();

            var contentWithCharset = new StringContent("{ A: 'B' }", Encoding.UTF8, "application/json");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, pathTemplate)
            {
                Content = contentWithCharset
            };

            Action validate = () => requestValidator.Validate(
                httpRequestMessage,
                openApi,
                pathTemplate);

            validate.Should().NotThrow();
        }

        [Test]
        public void ValidateContentType_WithInvalidValues_ShouldThrowExceptionWithCompleteContentTypeAndSimplifiedOne()
        {
            const string pathTemplate = "/path";

            var requestValidator = new RequestValidator(new[] {new JsonContentValidator()});

            var openApi = new OpenApiDocumentBuilder()
                .WithOperation(OperationType.Get)
                .WithPath(pathTemplate)
                .WithContentType("application/json; charset=utf-8")
                .Build();

            var contentIncompatible = new StringContent("{ A: 'B' }", Encoding.UTF8, "text/plain");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, pathTemplate)
            {
                Content = contentIncompatible
            };

            Action validate = () => requestValidator.Validate(
                httpRequestMessage,
                openApi,
                pathTemplate);

            validate.Should().Throw<Exception>()
                .WithMessage(
                    "Neither Content media type 'text/plain' or 'text/plain; charset=utf-8' are specified.");
        }
    }
}