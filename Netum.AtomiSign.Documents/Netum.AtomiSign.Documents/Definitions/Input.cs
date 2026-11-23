using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.Documents.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Documents operation to execute.
    /// </summary>
    /// <example>AddDocument</example>
    [DefaultValue(Method.AddDocument)]
    public Method Method { get; set; } = Method.AddDocument;

    /// <summary>
    /// Records API endpoint path relative to BaseUrl.
    /// </summary>
    /// <example>/records</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("/records")]
    public string Endpoint { get; set; } = "/records";

    /// <summary>
    /// ID of the record.
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    [DisplayFormat(DataFormatString = "Text")]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "RecordId must be a valid UUID.")]
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the document.
    /// </summary>
    /// <example>22222222-2222-2222-2222-222222222222</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.UpdateDocument, Method.GetDocumentByID, Method.DeleteDocument)]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "DocumentId must be a valid UUID.")]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Document file bytes.
    /// </summary>
    /// <example>byte[]</example>
    [DisplayFormat(DataFormatString = "Expression")]
    [UIHint(nameof(Method), "", Method.AddDocument, Method.UpdateDocument)]
    public byte[] File { get; set; } = [];

    /// <summary>
    /// File name to send with the document.
    /// </summary>
    /// <example>agreement.pdf</example>
    [DisplayFormat(DataFormatString = "Text")]
    [MaxLength(255)]
    [UIHint(nameof(Method), "", Method.AddDocument, Method.UpdateDocument)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Document order.
    /// </summary>
    /// <example>1</example>
    [UIHint(nameof(Method), "", Method.AddDocument, Method.UpdateDocument)]
    public decimal? Order { get; set; }

    /// <summary>
    /// Accept header value for the expected response.
    /// </summary>
    /// <example>application/json</example>
    [DefaultValue("application/json")]
    public string Accept { get; set; } = "application/json";
}
