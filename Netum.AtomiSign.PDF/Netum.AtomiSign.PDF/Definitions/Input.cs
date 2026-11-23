using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.PDF.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// API endpoint path relative to BaseUrl.
    /// </summary>
    /// <example>/pdf/convert</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("/pdf/convert")]
    [Required]
    public string Endpoint { get; set; } = "/pdf/convert";

    /// <summary>
    /// Text content to convert to PDF/A.
    /// </summary>
    /// <example>Hello from AtomiSign</example>
    [DisplayFormat(DataFormatString = "Text")]
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional title for the converted PDF/A file.
    /// </summary>
    /// <example>Converted document</example>
    [DisplayFormat(DataFormatString = "Text")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Accept header value for the expected response.
    /// </summary>
    /// <example>application/pdf</example>
    [DefaultValue("application/pdf")]
    public string Accept { get; set; } = "application/pdf";
}
