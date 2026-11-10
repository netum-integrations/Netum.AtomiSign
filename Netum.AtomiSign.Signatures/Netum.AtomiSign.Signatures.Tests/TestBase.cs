using Netum.AtomiSign.Signatures.Definitions;

namespace Netum.AtomiSign.Signatures.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
