namespace Netum.AtomiSign.Records.Definitions;

/// <summary>
/// Signature page mode.
/// </summary>
public enum SignaturePage
{
    /// <summary>
    /// Do not add a signature page.
    /// </summary>
    NoPage,

    /// <summary>
    /// Add a signature page.
    /// </summary>
    SignaturePage,

    /// <summary>
    /// Use signature fields.
    /// </summary>
    SignatureFields,
}
