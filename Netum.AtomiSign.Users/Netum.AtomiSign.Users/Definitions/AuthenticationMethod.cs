namespace Netum.AtomiSign.Users.Definitions;

/// <summary>
/// Request authentication method.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Authenticate with API token as bearer token.
    /// </summary>
    ApiToken,

    /// <summary>
    /// Authenticate with basic authentication.
    /// </summary>
    Basic,
}
