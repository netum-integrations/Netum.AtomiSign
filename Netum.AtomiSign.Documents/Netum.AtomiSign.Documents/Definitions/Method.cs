namespace Netum.AtomiSign.Documents.Definitions;

/// <summary>
/// AtomiSign documents operation.
/// </summary>
public enum Method
{
    /// <summary>
    /// POST /records/{recordId}/documents.
    /// </summary>
    AddDocument,

    /// <summary>
    /// PUT /records/{recordId}/documents/{documentId}.
    /// </summary>
    UpdateDocument,

    /// <summary>
    /// GET /records/{recordId}/documents/{documentId}.
    /// </summary>
    GetDocumentByID,

    /// <summary>
    /// DELETE /records/{recordId}/documents/{documentId}.
    /// </summary>
    DeleteDocument,
}
