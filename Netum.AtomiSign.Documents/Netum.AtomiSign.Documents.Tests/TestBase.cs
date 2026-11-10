using Netum.AtomiSign.Documents.Definitions;

namespace Netum.AtomiSign.Documents.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
