namespace Netum.AtomiSign.Records.Definitions;

/// <summary>
/// AtomiSign records operation.
/// </summary>
public enum Method
{
    /// <summary>
    /// POST /records.
    /// </summary>
    CreateRecord,

    /// <summary>
    /// GET /records.
    /// </summary>
    ReturnAllRecords,

    /// <summary>
    /// GET /records/{recordId}.
    /// </summary>
    GetRecordByID,

    /// <summary>
    /// PUT /records/{recordId}.
    /// </summary>
    ReplaceRecord,

    /// <summary>
    /// DELETE /records/{recordId}.
    /// </summary>
    DeleteRecord,
}
