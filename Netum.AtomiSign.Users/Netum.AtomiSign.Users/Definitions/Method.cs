namespace Netum.AtomiSign.Users.Definitions;

/// <summary>
/// AtomiSign users operation.
/// </summary>
public enum Method
{
    /// <summary>
    /// GET /users.
    /// </summary>
    GetUsers,

    /// <summary>
    /// DELETE /users/{userId}.
    /// </summary>
    DeleteUserByID,
}
