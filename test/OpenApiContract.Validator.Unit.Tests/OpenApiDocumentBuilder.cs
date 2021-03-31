using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace OpenApiContract.Validator.Unit.Tests
{
    public class OpenApiDocumentBuilder
    {
        private string _path = "/path";
        private string _contentType = "application/json";
        private OperationType _operationType = OperationType.Get;

        public OpenApiDocumentBuilder WithOperation(OperationType operationType)
        {
            _operationType = operationType;
            return this;
        }

        public OpenApiDocumentBuilder WithContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public OpenApiDocumentBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public OpenApiDocument Build() =>
            new()
            {
                Paths = new OpenApiPaths
                {
                    {
                        _path, new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                {
                                    _operationType, new OpenApiOperation
                                    {
                                        RequestBody = new OpenApiRequestBody
                                        {
                                            Content = new Dictionary<string, OpenApiMediaType>
                                            {
                                                {
                                                    _contentType, new OpenApiMediaType()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
    }
}