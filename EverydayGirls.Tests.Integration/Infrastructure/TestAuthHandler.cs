using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EverydayGirls.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Authentication handler for integration tests.
    /// Authenticates requests based on X-Test-UserId and X-Test-Email headers.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";
        public const string UserIdHeader = "X-Test-UserId";
        public const string EmailHeader = "X-Test-Email";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for test authentication headers
            if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues) ||
                !Request.Headers.TryGetValue(EmailHeader, out var emailValues))
            {
                // No test headers = not authenticated
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = userIdValues.ToString();
            var email = emailValues.ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid test authentication headers"));
            }

            // Create claims for the test user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
