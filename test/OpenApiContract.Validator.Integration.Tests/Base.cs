using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System;
using System.Net.Http;

namespace OpenApiContract.Validator.Integration.Tests
{
    public class Base
    {
        protected const string host = "http://meuhost";
        protected HttpClient httpClient;
        protected Faker Faker;

        [SetUp]
        public void SetUpBase()
        {
            Faker = new Faker();
            httpClient = Setup.Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri(host)
            });
        }

        [TearDown]
        public void TearDownBase()
        {
            httpClient.Dispose();
        }
    }
}
