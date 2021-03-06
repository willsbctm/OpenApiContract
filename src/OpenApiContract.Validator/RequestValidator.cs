using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.OpenApi.Models;

namespace OpenApiContract.Validator
{
    public class RequestValidator
    {
        private readonly IEnumerable<IContentValidator> _contentValidators;

        public RequestValidator(IEnumerable<IContentValidator> contentValidators)
        {
            _contentValidators = contentValidators;
        }

        public void Validate(
            HttpRequestMessage request,
            OpenApiDocument openApiDocument,
            string pathTemplate)
        {
            // Convert to absolute Uri as a workaround to limitation with Uri class - i.e. most of it's methods are not supported for relative Uri's.
            var requestUri = new Uri(new Uri("http://tempuri.org"), request.RequestUri);

            if (!TryGetOperation(openApiDocument, pathTemplate, request, out var operationType))
                throw new RequestDoesNotMatchSpecException($"Request URI '{requestUri.AbsolutePath}' does not allow '{operationType}' verb");

            var operationSpec = openApiDocument.GetOperationByPathAndType(pathTemplate, operationType, out OpenApiPathItem pathSpec);
            var parameterSpecs = ExpandParameterSpecs(pathSpec, operationSpec, openApiDocument);

            if (!TryParsePathNameValues(pathTemplate, requestUri.AbsolutePath, openApiDocument, out NameValueCollection pathNameValues))
                throw new RequestDoesNotMatchSpecException($"Request URI '{requestUri.AbsolutePath}' does not match specified template '{pathTemplate}'");

            if (request.Method != new HttpMethod(operationType.ToString()))
                throw new RequestDoesNotMatchSpecException($"Request method '{request.Method}' does not match specified operation type '{operationType}'");

            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Path), openApiDocument, pathNameValues);
            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Query), openApiDocument, HttpUtility.ParseQueryString(requestUri.Query));
            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Header), openApiDocument, request.Headers.ToNameValueCollection());

            if (operationSpec.RequestBody != null)
                ValidateContent(operationSpec.RequestBody, openApiDocument, request.Content);
        }

        private static IEnumerable<OpenApiParameter> ExpandParameterSpecs(
            OpenApiPathItem pathSpec,
            OpenApiOperation operationSpec,
            OpenApiDocument openApiDocument)
        {
            var securityParameterSpecs = DeriveSecurityParameterSpecs(operationSpec, openApiDocument);

            return securityParameterSpecs
                .Concat(pathSpec.Parameters)
                .Concat(operationSpec.Parameters)
                .Select(p =>
                {
                    return (p.Reference != null)
                        ? (OpenApiParameter)openApiDocument.ResolveReference(p.Reference)
                        : p;
                });
        }

        private static IEnumerable<OpenApiParameter> DeriveSecurityParameterSpecs(
            OpenApiOperation operationSpec,
            OpenApiDocument openApiDocument)
        {
            // TODO
            return new OpenApiParameter[] { };
        }

        private bool TryParsePathNameValues(string pathTemplate, string requestUri, OpenApiDocument openApiDocument, out NameValueCollection pathNameValues)
        {
            pathNameValues = new NameValueCollection();

            if (!openApiDocument.Paths.ContainsKey(pathTemplate))
                return false;

            requestUri = RemoveBasePath(requestUri, openApiDocument);

            var templateMatcher = new TemplateMatcher(TemplateParser.Parse(pathTemplate), null);
            var routeValues = new RouteValueDictionary();
            if (!templateMatcher.TryMatch(new PathString(requestUri), routeValues))
                return false;

            foreach (var entry in routeValues)
            {
                pathNameValues.Add(entry.Key, entry.Value.ToString());
            }

            return true;
        }

        private bool TryGetOperation(OpenApiDocument openApiDocument, string pathTemplate, HttpRequestMessage request, out OperationType operation)
        {
            operation = ObterVerboOpenApi(request);
            return openApiDocument.Paths[pathTemplate].Operations.ContainsKey(operation);
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

        private string RemoveBasePath(string requestUri, OpenApiDocument openApiDocument)
        {
            var basesPath = openApiDocument.Servers.Select(s => new Uri(s.Url)).ToList();
            basesPath.ForEach(uri =>
            {
                var indexBasePath = requestUri.IndexOf(uri.AbsolutePath);
                if (indexBasePath > -1)
                {
                    requestUri = requestUri.Remove(indexBasePath, uri.AbsolutePath.Length);
                    return;
                }
            });

            return requestUri;
        }

        private void ValidateParameters(
            IEnumerable<OpenApiParameter> parameterSpecs,
            OpenApiDocument openApiDocument,
            NameValueCollection parameterNameValues)
        {
            foreach (var parameterSpec in parameterSpecs)
            {
                var value = parameterNameValues[parameterSpec.Name];

                if ((parameterSpec.In == ParameterLocation.Path || parameterSpec.Required) && value == null)
                    throw new RequestDoesNotMatchSpecException($"Required parameter '{parameterSpec.Name}' is not present");

                if (value == null || parameterSpec.Schema == null)
                    continue;

                var schema = (parameterSpec.Schema.Reference != null)
                    ? (OpenApiSchema)openApiDocument.ResolveReference(parameterSpec.Schema.Reference)
                    : parameterSpec.Schema;

                if (!schema.TryParse(value, out object typedValue))
                    throw new RequestDoesNotMatchSpecException($"Parameter '{parameterSpec.Name}' is not of type '{parameterSpec.Schema.TypeIdentifier()}'");
            }
        }

        private void ValidateContent(OpenApiRequestBody requestBodySpec, OpenApiDocument openApiDocument, HttpContent content)
        {
            requestBodySpec = (requestBodySpec.Reference != null)
                ? (OpenApiRequestBody)openApiDocument.ResolveReference(requestBodySpec.Reference)
                : requestBodySpec;

            if (requestBodySpec.Required && content == null)
                throw new RequestDoesNotMatchSpecException("Required content is not present");

            if (content == null)
                return;

            var mediaTypeSpec = ValidateAndExtractOpenApiMediaType(requestBodySpec, content);

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
                throw new RequestDoesNotMatchSpecException($"Content does not match spec. {contentException.Message}");
            }
        }

        private static OpenApiMediaType ValidateAndExtractOpenApiMediaType(OpenApiRequestBody requestBodySpec,
            HttpContent content)
        {
            if (requestBodySpec.Content.TryGetValue(content.Headers.ContentType.MediaType, out var mediaTypeSpec))
                return mediaTypeSpec;
            
            if (requestBodySpec.Content.TryGetValue(content.Headers.ContentType.ToString(), out mediaTypeSpec))
                return mediaTypeSpec;
            
            throw new RequestDoesNotMatchSpecException(
                $"Neither Content media type '{content.Headers.ContentType.MediaType}' or '{content.Headers.ContentType}' are specified.");
        }
    }

    public class RequestDoesNotMatchSpecException : Exception
    {
        public RequestDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}
