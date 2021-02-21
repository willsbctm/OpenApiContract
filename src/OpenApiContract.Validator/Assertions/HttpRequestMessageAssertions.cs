using Microsoft.OpenApi.Models;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace OpenApiContract.Validator.Assertions
{
    public class HttpRequestMessageAssertions :
    ReferenceTypeAssertions<HttpRequestMessage, HttpRequestMessageAssertions>
    {
        public HttpRequestMessageAssertions(HttpRequestMessage instance) => Subject = instance;

        protected override string Identifier => "HttpRequest";

        public AndConstraint<HttpRequestMessageAssertions> SatisfyEspecification(
            OpenApiDocument openApiDocument, string pathTemplateExpected)
        {
            var runnerOptions = new ApiTestRunnerOptions();
            var validatorRequest = new RequestValidator(runnerOptions.ContentValidators);

            validatorRequest.Validate(Subject, openApiDocument, pathTemplateExpected);

            return new AndConstraint<HttpRequestMessageAssertions>(this);
        }
    }
}