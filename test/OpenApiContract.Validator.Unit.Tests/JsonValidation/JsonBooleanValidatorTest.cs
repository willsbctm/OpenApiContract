using System;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenApiContract.Validator.JsonValidation;

namespace OpenApiContract.Validator.Unit.Tests.JsonValidation
{
    public class JsonBooleanValidatorTest
    {
        private JsonBooleanValidator _validator;

        [SetUp]
        public void Setup() => _validator = new JsonBooleanValidator();

        [Test]
        public void CanValidate_WithSchemaTypeBoolean_ShouldReturnTrue() =>
            _validator
                .CanValidate(new OpenApiSchema { Type = "boolean" })
                .Should()
                .BeTrue();

        [Test]
        public void Validate_WithBooleanToken_ShouldReturnTrue()
        {
            var booleanToken = JToken.Parse("true");
            
            var result = _validator.Validate(null, null, booleanToken, out var errorMessages);

            (result, errorMessages).Should().BeEquivalentTo((true, Array.Empty<string>()));
        }

        [Test]
        [TestCase("'string'")]
        [TestCase(" { object: true }")]
        [TestCase("123")]
        [TestCase("123.1")]
        public void Validate_NotBooleanToken_ShouldReturnFalse(string notBooleanToken)
        {
            var booleanToken = JToken.Parse(notBooleanToken);
            
            var result = _validator.Validate(null, null, booleanToken, out var errorMessages);

            (result, errorMessages).Should().BeEquivalentTo((false, new[] { "Path: . Instance is not of type 'boolean'"}));
        }

        [Test]
        [TestCase("Boolean")]
        [TestCase("bool")]
        [TestCase("Bool")]
        public void CanValidate_SimilarToBooleanButNotCorrent_ShouldReturnFalse(string notBooleanTypes) =>
            _validator
                .CanValidate(new OpenApiSchema { Type = notBooleanTypes })
                .Should()
                .BeFalse();
    }
}