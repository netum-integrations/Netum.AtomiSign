namespace Netum.AtomiSign.PDF.Definitions;

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
    /// Response body bytes returned by the API.
    /// </summary>
    /// <example>byte[]</example>
    public byte[] BodyBytes { get; set; } = [];

    /// <summary>
    /// Body size of response in megabytes.
    /// </summary>
    /// <example>1.23</example>
    public double BodySizeInMegaBytes => System.Math.Round(BodyBytes?.Length / (1024 * 1024d) ?? 0, 3);

    /// <summary>
    /// Response content type.
    /// </summary>
    /// <example>application/pdf</example>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Response headers.
    /// </summary>
    /// <example>{[ "content-type": "application/pdf", ...]}</example>
    public System.Collections.Generic.Dictionary<string, string> Headers { get; set; } = [];

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
