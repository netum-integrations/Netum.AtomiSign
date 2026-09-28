using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Records.Definitions;
using NUnit.Framework;

namespace Netum.AtomiSign.Records.Tests;

[TestFixture]
internal class TaskTests : TestBase
{
    [Test]
    public async Task CreateRecord_ShouldPostJsonBody()
    {
        using var server = new TestHttpServer("{\"id\":\"record\"}");
        var input = ValidCreateInput();
        input.Agents = "[{\"givenName\":\"Testinimi\",\"familyName\":\"Testisukunimi\",\"email\":\"testi@example.com\",\"role\":\"author\"}]";
        input.RecordAuthentication = RecordAuthentication.Strong;
        input.SigningOrder = SigningOrder.Sequential;
        input.SignaturePage = SignaturePage.SignatureFields;

        var result = await AtomiSign.Records(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("POST"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo("/records"));
        Assert.That(server.Request.Headers["Authorization"], Is.EqualTo("Bearer token"));

        using var json = JsonDocument.Parse(server.Request.Body);
        var root = json.RootElement;
        Assert.That(root.GetProperty("authentication").GetString(), Is.EqualTo("strong"));
        Assert.That(root.GetProperty("signingOrder").GetString(), Is.EqualTo("sequential"));
        Assert.That(root.GetProperty("signaturePage").GetString(), Is.EqualTo("signature-fields"));
        Assert.That(root.GetProperty("agents")[0].GetProperty("givenName").GetString(), Is.EqualTo("Testinimi"));
    }

    [Test]
    public async Task ReturnAllRecords_ShouldSendQueryParameters()
    {
        using var server = new TestHttpServer("[]");
        var input = new Input
        {
            Method = Method.ReturnAllRecords,
            Term = "agreement",
            Statuses = ["draft", "completed"],
            Own = true,
            UpdatedMin = new DateTime(2026, 6, 17, 8, 0, 0, DateTimeKind.Utc),
            QueryStatus = "waiting_me",
            Offset = 5,
            Limit = 25,
        };

        var result = await AtomiSign.Records(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("GET"));
        Assert.That(server.Request.PathAndQuery, Does.StartWith("/records?"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("term=agreement"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("statuses=draft"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("statuses=completed"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("own=true"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("status=waiting_me"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("offset=5"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("limit=25"));
    }

    [TestCase(Method.GetRecordByID, "GET")]
    [TestCase(Method.DeleteRecord, "DELETE")]
    public async Task RecordIdOperations_ShouldAppendRecordId(Method method, string expectedHttpMethod)
    {
        using var server = new TestHttpServer();
        var input = new Input
        {
            Method = method,
            RecordId = "11111111-1111-1111-1111-111111111111",
        };

        var result = await AtomiSign.Records(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo(expectedHttpMethod));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo("/records/11111111-1111-1111-1111-111111111111"));
    }

    [Test]
    public async Task ReplaceRecord_ShouldUsePutAndBasicAuthentication()
    {
        using var server = new TestHttpServer();
        var input = ValidCreateInput();
        input.Method = Method.ReplaceRecord;
        input.RecordId = "11111111-1111-1111-1111-111111111111";
        input.Status = 1;

        var result = await AtomiSign.Records(input, BasicConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("PUT"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo("/records/11111111-1111-1111-1111-111111111111"));
        Assert.That(server.Request.Headers["Authorization"], Does.StartWith("Basic "));
        Assert.That(server.Request.Body, Does.Contain("\"status\":1"));
    }

    [Test]
    public async Task InvalidAgentsJson_ShouldReturnFailedResult()
    {
        var input = ValidCreateInput();
        input.Agents = "{}";
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await AtomiSign.Records(input, ApiTokenConnection("http://127.0.0.1:9"), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Agents must be a JSON array"));
    }

    private static Input ValidCreateInput() => new()
    {
        Method = Method.CreateRecord,
        Name = "Agreement",
        Agents = "[{\"givenName\":\"Jane\",\"familyName\":\"Doe\",\"email\":\"jane.doe@example.com\",\"role\":\"author\"}]",
    };

    private static Connection ApiTokenConnection(string baseUrl) => new()
    {
        BaseUrl = baseUrl,
        Authentication = AuthenticationMethod.ApiToken,
        ApiToken = "token",
    };

    private static Connection BasicConnection(string baseUrl) => new()
    {
        BaseUrl = baseUrl,
        Authentication = AuthenticationMethod.Basic,
        Username = "user",
        Password = "pass",
    };
}
