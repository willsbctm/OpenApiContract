using System;

namespace OpenApiContract.Validator
{
    internal class ContentDoesNotMatchSpecException : Exception
    {
        public ContentDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}