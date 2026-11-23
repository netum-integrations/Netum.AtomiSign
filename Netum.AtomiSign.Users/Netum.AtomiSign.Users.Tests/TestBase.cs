using Netum.AtomiSign.Users.Definitions;

namespace Netum.AtomiSign.Users.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
