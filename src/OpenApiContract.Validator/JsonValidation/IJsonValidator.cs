using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace OpenApiContract.Validator.JsonValidation
{
    public interface IJsonValidator
    {
        bool CanValidate(OpenApiSchema schema);

        bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages);
    }
}