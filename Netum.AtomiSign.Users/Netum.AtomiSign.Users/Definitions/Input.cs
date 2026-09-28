using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.Users.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Users operation to execute.
    /// </summary>
    /// <example>GetUsers</example>
    [DefaultValue(Method.GetUsers)]
    public Method Method { get; set; } = Method.GetUsers;

    /// <summary>
    /// API endpoint path relative to BaseUrl.
    /// </summary>
    /// <example>/users</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("/users")]
    public string Endpoint { get; set; } = "/users";

    /// <summary>
    /// AtomiSign ID of the user.
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.DeleteUserByID)]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "UserId must be a valid UUID.")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Filter by external ID.
    /// </summary>
    /// <example>external-123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.GetUsers)]
    public string ExtId { get; set; } = string.Empty;

    /// <summary>
    /// Filter by AtomiSign ID.
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.GetUsers)]
    public string AtomiSignId { get; set; } = string.Empty;

    /// <summary>
    /// Filter by email.
    /// </summary>
    /// <example>user@example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.GetUsers)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Pagination offset.
    /// </summary>
    /// <example>0</example>
    [DefaultValue(0)]
    [Range(0, int.MaxValue)]
    [UIHint(nameof(Method), "", Method.GetUsers)]
    public int? Offset { get; set; }

    /// <summary>
    /// Pagination limit.
    /// </summary>
    /// <example>10</example>
    [DefaultValue(10)]
    [Range(1, int.MaxValue)]
    [UIHint(nameof(Method), "", Method.GetUsers)]
    public int? Limit { get; set; }

    /// <summary>
    /// Accept header value for the expected response.
    /// </summary>
    /// <example>application/json</example>
    [DefaultValue("application/json")]
    public string Accept { get; set; } = "application/json";
}
