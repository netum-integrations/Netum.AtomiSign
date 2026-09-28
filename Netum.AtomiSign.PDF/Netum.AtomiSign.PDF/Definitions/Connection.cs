using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.PDF.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Base URL of the AtomiSign API.
    /// </summary>
    /// <example>https://atomisign.fi</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("https://atomisign.fi")]
    [Required]
    public string BaseUrl { get; set; } = "https://atomisign.fi";

    /// <summary>
    /// Method of authenticating the request.
    /// </summary>
    /// <example>ApiToken</example>
    [DefaultValue(AuthenticationMethod.ApiToken)]
    public AuthenticationMethod Authentication { get; set; } = AuthenticationMethod.ApiToken;

    /// <summary>
    /// API token. Sent as a bearer token.
    /// </summary>
    /// <example>Token123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", AuthenticationMethod.ApiToken)]
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Username for basic authentication.
    /// </summary>
    /// <example>Username</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Authentication), "", AuthenticationMethod.Basic)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for basic authentication.
    /// </summary>
    /// <example>Password123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", AuthenticationMethod.Basic)]
    public string Password { get; set; } = string.Empty;
}
