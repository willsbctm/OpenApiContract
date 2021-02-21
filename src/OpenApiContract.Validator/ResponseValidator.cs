using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.OpenApi.Models;

namespace OpenApiContract.Validator
{
    internal class ResponseValidator
    {
        private readonly IEnumerable<IContentValidator> _contentValidators;

        public ResponseValidator(IEnumerable<IContentValidator> contentValidators)
        {
            _contentValidators = contentValidators;
        }

        public static OperationType ObterVerboOpenApi(HttpRequestMessage requisicao) => requisicao.Method.ToString() switch
        {
            "GET" => OperationType.Get,
            "PUT" => OperationType.Put,
            "POST" => OperationType.Post,
            "DELETE" => OperationType.Delete,
            "OPTIONS" => OperationType.Options,
            "HEAD" => OperationType.Head,
            "PATCH" => OperationType.Patch,
            "TRACE" => OperationType.Trace,
            _ => throw new NotImplementedException()
        };


        private bool TryGetOperation(OpenApiDocument openApiDocument, string pathTemplate, HttpRequestMessage request, out OperationType operation)
        {
            operation = ObterVerboOpenApi(request);
            return openApiDocument.Paths[pathTemplate].Operations.ContainsKey(operation);
        }

        public void Validate(
            HttpResponseMessage response,
            HttpRequestMessage request,
            OpenApiDocument openApiDocument,
            string pathTemplate,
            HttpStatusCode expectedStatusCode)
        {
            var requestUri = new Uri(new Uri("http://tempuri.org"), request.RequestUri);

            if (!TryGetOperation(openApiDocument, pathTemplate, request, out var operationType))
                throw new RequestDoesNotMatchSpecException($"Request URI '{requestUri.AbsolutePath}' does not allow '{operationType}' verb");

            var operationSpec = openApiDocument.GetOperationByPathAndType(pathTemplate, operationType, out _);
            if (!operationSpec.Responses.TryGetValue(((int)expectedStatusCode).ToString(), out OpenApiResponse responseSpec))
                throw new InvalidOperationException($"Response for status '{expectedStatusCode}' not found for operation '{operationSpec.OperationId}'");

            if (response.StatusCode != expectedStatusCode)
                throw new ResponseDoesNotMatchSpecException($"Status code '{response.StatusCode}' does not match expected value '{expectedStatusCode}'");

            ValidateHeaders(responseSpec.Headers, openApiDocument, response.Headers.ToNameValueCollection());

            if (responseSpec.Content != null && responseSpec.Content.Keys.Any())
                ValidateContent(responseSpec.Content, openApiDocument, response.Content);
        }

        private void ValidateHeaders(
            IDictionary<string, OpenApiHeader> headerSpecs,
            OpenApiDocument openApiDocument,
            NameValueCollection headerValues)
        {
            foreach (var entry in headerSpecs)
            {
                var value = headerValues[entry.Key];
                var headerSpec = entry.Value;

                if (headerSpec.Required && value == null)
                    throw new ResponseDoesNotMatchSpecException($"Required header '{entry.Key}' is not present");

                if (value == null || headerSpec.Schema == null)
                    continue;

                var schema = (headerSpec.Schema.Reference != null)
                    ? (OpenApiSchema)openApiDocument.ResolveReference(headerSpec.Schema.Reference)
                    : headerSpec.Schema;

                if (value == null)
                    continue;

                if (!schema.TryParse(value, out object typedValue))
                    throw new ResponseDoesNotMatchSpecException($"Header '{entry.Key}' is not of type '{headerSpec.Schema.TypeIdentifier()}'");
            }
        }

        private void ValidateContent(
            IDictionary<string, OpenApiMediaType> contentSpecs,
            OpenApiDocument openApiDocument,
            HttpContent content)
        {
            if (content == null)
                return;

            if (!contentSpecs.TryGetValue(content.Headers.ContentType.MediaType, out OpenApiMediaType mediaTypeSpec))
                throw new ResponseDoesNotMatchSpecException($"Content media type '{content.Headers.ContentType.MediaType}' is not specified");

            try
            {
                foreach (var contentValidator in _contentValidators)
                {
                    if (contentValidator.CanValidate(content.Headers.ContentType.MediaType))
                        contentValidator.Validate(mediaTypeSpec, openApiDocument, content);
                }
            }
            catch (ContentDoesNotMatchSpecException contentException)
            {
                throw new ResponseDoesNotMatchSpecException($"Content does not match spec. {contentException.Message}");
            }
        }
    }

    public class ResponseDoesNotMatchSpecException : Exception
    {
        public ResponseDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}
