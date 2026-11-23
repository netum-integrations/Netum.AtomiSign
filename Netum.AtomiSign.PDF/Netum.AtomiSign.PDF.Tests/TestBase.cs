using Netum.AtomiSign.PDF.Definitions;

namespace Netum.AtomiSign.PDF.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
