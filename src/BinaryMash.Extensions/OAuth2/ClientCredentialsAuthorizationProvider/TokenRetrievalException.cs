namespace BinaryMash.Extensions.OAuth2.ClientCredentialsAuthorizationProvider
{
    public class TokenRetrievalException : Exception
    {
        public TokenRetrievalException() : base()
        {
        }

        public TokenRetrievalException(string? message) : base(message)
        {
        }

        public TokenRetrievalException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
