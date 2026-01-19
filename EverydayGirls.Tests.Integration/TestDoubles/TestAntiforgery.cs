using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace EverydayGirls.Tests.Integration.TestDoubles
{
    /// <summary>
    /// Test double for IAntiforgery that always considers requests valid.
    /// </summary>
    public class TestAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet("test-request-token", "test-cookie-token", "test-form-field", "test-header");
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet("test-request-token", "test-cookie-token", "test-form-field", "test-header");
        }

        public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public Task ValidateRequestAsync(HttpContext httpContext)
        {
            // Always valid - no-op
            return Task.CompletedTask;
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {
            // No-op
        }
    }
}
