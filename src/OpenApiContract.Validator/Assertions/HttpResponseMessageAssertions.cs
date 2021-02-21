using Microsoft.OpenApi.Models;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Primitives;
using System.Net;

namespace OpenApiContract.Validator.Assertions
{
    public class HttpResponseMessageAssertions :
        ReferenceTypeAssertions<HttpResponseMessage, HttpResponseMessageAssertions>
    {
        public HttpResponseMessageAssertions(HttpResponseMessage instance) => Subject = instance;

        protected override string Identifier => "HttpResponse";

        public AndConstraint<HttpResponseMessageAssertions> SatisfyEspecification(HttpRequestMessage httpRequestMessage,
            OpenApiDocument openApiDocument, string pathTemplateExpected, HttpStatusCode codeExpected)
        {
            var runnerOptions = new ApiTestRunnerOptions();
            var validatorResponse = new ResponseValidator(runnerOptions.ContentValidators);

            validatorResponse.Validate(Subject, httpRequestMessage, openApiDocument, pathTemplateExpected, codeExpected);

            return new AndConstraint<HttpResponseMessageAssertions>(this);
        }
    }
}