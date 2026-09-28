using System.Collections.Generic;

namespace Netum.AtomiSign.Documents.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Response body returned by the API.
    /// </summary>
    /// <example>{ "id": "22222222-2222-2222-2222-222222222222" }</example>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Response content type.
    /// </summary>
    /// <example>application/json</example>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Response headers.
    /// </summary>
    /// <example>{[ "content-type": "application/json", ...]}</example>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// HTTP status code.
    /// </summary>
    /// <example>200</example>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
