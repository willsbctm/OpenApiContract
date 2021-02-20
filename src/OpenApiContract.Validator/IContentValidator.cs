﻿using System;
using System.Net.Http;
using Microsoft.OpenApi.Models;

namespace OpenApiContract.Validator
{
    public interface IContentValidator
    {
        bool CanValidate(string mediaType);

        void Validate(OpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content);
    }

    public class ContentDoesNotMatchSpecException : Exception
    {
        public ContentDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}
