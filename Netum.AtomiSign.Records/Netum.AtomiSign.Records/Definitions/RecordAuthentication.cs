namespace Netum.AtomiSign.Records.Definitions;

/// <summary>
/// Record authentication level.
/// </summary>
public enum RecordAuthentication
{
    /// <summary>
    /// Light authentication.
    /// </summary>
    Light,

    /// <summary>
    /// SMS authentication.
    /// </summary>
    Sms,

    /// <summary>
    /// Strong authentication.
    /// </summary>
    Strong,
}
