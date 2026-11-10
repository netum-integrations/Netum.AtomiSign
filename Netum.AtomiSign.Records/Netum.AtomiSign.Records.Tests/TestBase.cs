using Netum.AtomiSign.Records.Definitions;

namespace Netum.AtomiSign.Records.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
