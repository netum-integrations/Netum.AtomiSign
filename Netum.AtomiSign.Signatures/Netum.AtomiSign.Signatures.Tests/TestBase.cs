using System;
using dotenv.net;
using Netum.AtomiSign.Signatures.Definitions;

namespace Netum.AtomiSign.Signatures.Tests;

internal abstract class TestBase
{
    internal TestBase()
    {
        DotEnv.Load();
        SecretKey = Environment.GetEnvironmentVariable("FRENDS_SECRET_KEY");
    }

    protected string SecretKey { get; set; }

    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
