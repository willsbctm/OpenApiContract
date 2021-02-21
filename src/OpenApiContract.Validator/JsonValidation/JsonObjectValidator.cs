using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace OpenApiContract.Validator.JsonValidation
{
    internal class JsonObjectValidator : IJsonValidator
    {
        private readonly IJsonValidator _jsonValidator;

        public JsonObjectValidator(IJsonValidator jsonValidator) => _jsonValidator = jsonValidator;

        public bool CanValidate(OpenApiSchema schema) => schema.Type == "object";

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type != JTokenType.Object)
            {
                errorMessages = new[] { $"Path: {instance.Path}. Instance is not of type 'object'" };
                return false;
            }

            var jObject = (JObject)instance;
            var properties = jObject.Properties();
            var errorMessagesList = new List<string>();

            // maxProperties
            if (schema.MaxProperties.HasValue && properties.Count() > schema.MaxProperties.Value)
                errorMessagesList.Add($"Path: {instance.Path}. Number of properties is greater than maxProperties");

            // minProperties
            if (schema.MinProperties.HasValue && properties.Count() < schema.MinProperties.Value)
                errorMessagesList.Add($"Path: {instance.Path}. Number of properties is less than minProperties");

            // required
            var missingRequiredProperties = schema.Required.Where(x => !properties.Any(p => p.Name == x));
            if (schema.Required != null && missingRequiredProperties.Any())
                errorMessagesList.Add($"Path: {instance.Path}. Required property(s) not present: {string.Join(",", missingRequiredProperties)}");

            foreach (var property in properties)
            {
                // properties
                if (schema.Properties != null && schema.Properties.TryGetValue(property.Name, out OpenApiSchema propertySchema))
                {
                    if (property.HasValues && !string.IsNullOrEmpty(property.Value.ToString()))
                        if (!_jsonValidator.Validate(propertySchema, openApiDocument, property.Value, out IEnumerable<string> propertyErrorMessages))
                            errorMessagesList.AddRange(propertyErrorMessages);

                    continue;
                }
                else
                {
                    errorMessagesList.Add($"Property {property.Name} is not expected");
                }

                if (!schema.AdditionalPropertiesAllowed)
                    errorMessagesList.Add($"Path: {instance.Path}. Additional properties not allowed");

                // additionalProperties
                if (schema.AdditionalProperties != null)
                {
                    if (!_jsonValidator.Validate(schema.AdditionalProperties, openApiDocument, property.Value, out IEnumerable<string> propertyErrorMessages))
                        errorMessagesList.AddRange(propertyErrorMessages);
                }
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}
