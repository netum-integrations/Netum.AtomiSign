namespace Netum.AtomiSign.Records.Definitions;

/// <summary>
/// Record signing order.
/// </summary>
public enum SigningOrder
{
    /// <summary>
    /// Agents may sign in parallel.
    /// </summary>
    Parallel,

    /// <summary>
    /// Agents must sign sequentially.
    /// </summary>
    Sequential,
}
