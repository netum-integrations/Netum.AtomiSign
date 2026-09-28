using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.Signatures.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// API endpoint path relative to BaseUrl.
    /// </summary>
    /// <example>/signatures</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("/signatures")]
    [Required]
    public string Endpoint { get; set; } = "/signatures";

    /// <summary>
    /// File content to sign.
    /// </summary>
    /// <example>byte[]</example>
    [DisplayFormat(DataFormatString = "Expression")]
    public byte[] File { get; set; } = [];

    /// <summary>
    /// Signature level.
    /// </summary>
    /// <example>PAdES_BASELINE_B</example>
    [DefaultValue(SignatureLevel.PAdES_BASELINE_B)]
    public SignatureLevel SignatureLevel { get; set; } = SignatureLevel.PAdES_BASELINE_B;

    /// <summary>
    /// Accept header value for the expected response.
    /// </summary>
    /// <example>application/pdf</example>
    [DefaultValue("application/pdf")]
    public string Accept { get; set; } = "application/pdf";
}
