using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Netum.AtomiSign.Records.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Records operation to execute.
    /// </summary>
    /// <example>CreateRecord</example>
    [DefaultValue(Method.CreateRecord)]
    public Method Method { get; set; } = Method.CreateRecord;

    /// <summary>
    /// API endpoint path relative to BaseUrl.
    /// </summary>
    /// <example>/records</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("/records")]
    public string Endpoint { get; set; } = "/records";

    /// <summary>
    /// ID of the record.
    /// </summary>
    /// <example>12345678-1234-1234-1234-123456789012</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Method), "", Method.GetRecordByID, Method.ReplaceRecord, Method.DeleteRecord)]
    [RegularExpression("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "RecordId must be a valid UUID.")]
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the record.
    /// </summary>
    /// <example>Agreement 123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [MaxLength(255)]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The message to be sent by email and displayed to the user when signing or reviewing the request.
    /// </summary>
    /// <example>Please review and sign this request.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [MaxLength(2048)]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Authentication level.
    /// </summary>
    /// <example>Light</example>
    [DefaultValue(RecordAuthentication.Light)]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public RecordAuthentication RecordAuthentication { get; set; } = RecordAuthentication.Light;

    /// <summary>
    /// Record status. Used when replacing a record.
    /// </summary>
    /// <example>1</example>
    [UIHint(nameof(Method), "", Method.ReplaceRecord)]
    public int? Status { get; set; }

    /// <summary>
    /// Expiration date and time for the record.
    /// </summary>
    /// <example>2026-05-27T12:00:00Z</example>
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Optional reminder date and time.
    /// </summary>
    /// <example>2026-05-26T12:00:00Z</example>
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public DateTime? SendRemindersAt { get; set; }

    /// <summary>
    /// Metadata to be stored within the record.
    /// </summary>
    /// <example>{ "customerId": "12345" }</example>
    [DisplayFormat(DataFormatString = "Expression")]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public object Metadata { get; set; }

    /// <summary>
    /// Signing order.
    /// </summary>
    /// <example>Parallel</example>
    [DefaultValue(SigningOrder.Parallel)]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public SigningOrder SigningOrder { get; set; } = SigningOrder.Parallel;

    /// <summary>
    /// Signature page mode.
    /// </summary>
    /// <example>SignaturePage</example>
    [DefaultValue(SignaturePage.SignaturePage)]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public SignaturePage SignaturePage { get; set; } = SignaturePage.SignaturePage;

    /// <summary>
    /// Agents included in the record request body.
    /// </summary>
    /// <example>new { givenName = "Jane", familyName = "Doe", email = "jane.doe@example.com", role = "author" }</example>
    [DisplayFormat(DataFormatString = "Expression")]
    [UIHint(nameof(Method), "", Method.CreateRecord, Method.ReplaceRecord)]
    public string Agents { get; set; }

    /// <summary>
    /// The search term.
    /// </summary>
    /// <example>agreement</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public string Term { get; set; } = string.Empty;

    /// <summary>
    /// Record statuses to include in the query.
    /// </summary>
    /// <example>new [] { "draft", "completed" }</example>
    [DisplayFormat(DataFormatString = "Expression")]
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public string[] Statuses { get; set; } = [];

    /// <summary>
    /// Whether to return only records owned by the authenticated user.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public bool? Own { get; set; }

    /// <summary>
    /// Return records updated after this date and time.
    /// </summary>
    /// <example>2026-05-27T08:00:00Z</example>
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public DateTime? UpdatedMin { get; set; }

    /// <summary>
    /// Status filter. Allowed values are waiting_me and waiting_others.
    /// </summary>
    /// <example>waiting_me</example>
    [DisplayFormat(DataFormatString = "Text")]
    [RegularExpression("^(|waiting_me|waiting_others)$", ErrorMessage = "QueryStatus must be waiting_me or waiting_others.")]
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public string QueryStatus { get; set; } = string.Empty;

    /// <summary>
    /// The number of items to skip before starting to collect the result set.
    /// </summary>
    /// <example>0</example>
    [DefaultValue(0)]
    [Range(0, int.MaxValue)]
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public int Offset { get; set; }

    /// <summary>
    /// The number of items to return.
    /// </summary>
    /// <example>10</example>
    [DefaultValue(10)]
    [Range(1, 100)]
    [UIHint(nameof(Method), "", Method.ReturnAllRecords)]
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Accept header value for the expected response.
    /// </summary>
    /// <example>application/json</example>
    [DefaultValue("application/json")]
    public string Accept { get; set; } = "application/json";
}
