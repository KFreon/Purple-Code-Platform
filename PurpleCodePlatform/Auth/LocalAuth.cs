using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PurpleCodePlatform.Auth
{
    public class LocalAuthOptions : AuthenticationSchemeOptions
    {
        public static string SchemeName = "LocalAuth";
    }

    public class LocalAuthHandler : AuthenticationHandler<LocalAuthOptions>
    {
        public LocalAuthHandler(IOptionsMonitor<LocalAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var email = "kael.larkin@purple.telstra.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Name, email) }, "Bearer");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
    }
}
