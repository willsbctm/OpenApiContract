using Microsoft.OpenApi.Models;
using FluentAssertions;
using FluentAssertions.Primitives;
using OpenApiContract.Validator.Interceptors;
using System.Net;

namespace OpenApiContract.Validator.Assertions
{
    public class CallAssertions :
        ReferenceTypeAssertions<Call, CallAssertions>
    {
        public CallAssertions(Call instance) => Subject = instance;

        protected override string Identifier => "Call";

        public AndConstraint<CallAssertions> SatisfyEspecification(
            OpenApiDocument openApiDocument, string pathTemplateExpected, HttpStatusCode codeExpected)
        {
            var requestAssertions = new HttpRequestMessageAssertions(Subject.HttpRequest);
            requestAssertions.SatisfyEspecification(openApiDocument, pathTemplateExpected);

            var responseAsssertions = new HttpResponseMessageAssertions(Subject.HttpResponse);
            responseAsssertions.SatisfyEspecification(Subject.HttpRequest, openApiDocument, pathTemplateExpected, codeExpected);

            return new AndConstraint<CallAssertions>(this);
        }
    }

}