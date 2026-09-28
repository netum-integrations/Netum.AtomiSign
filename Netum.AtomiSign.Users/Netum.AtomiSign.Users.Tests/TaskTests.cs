using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Users.Definitions;
using NUnit.Framework;

namespace Netum.AtomiSign.Users.Tests;

[TestFixture]
internal class TaskTests : TestBase
{
    private const string UserId = "11111111-1111-1111-1111-111111111111";

    [Test]
    public async Task GetUsers_ShouldSendQueryParameters()
    {
        using var server = new TestHttpServer("[{\"email\":\"user@example.com\"}]");
        var input = new Input
        {
            Method = Method.GetUsers,
            ExtId = "external-1",
            AtomiSignId = UserId,
            Email = "user@example.com",
            Offset = 10,
            Limit = 50,
        };

        var result = await AtomiSign.Users(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("GET"));
        Assert.That(server.Request.PathAndQuery, Does.StartWith("/users?"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("extId=external-1"));
        Assert.That(server.Request.PathAndQuery, Does.Contain($"atomiSignId={UserId}"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("email=user%40example.com"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("offset=10"));
        Assert.That(server.Request.PathAndQuery, Does.Contain("limit=50"));
        Assert.That(server.Request.Headers["Authorization"], Is.EqualTo("Bearer token"));
    }

    [Test]
    public async Task GetUsers_ShouldOmitEmptyOptionalQueryParameters()
    {
        using var server = new TestHttpServer("[]");
        var input = new Input
        {
            Method = Method.GetUsers,
            ExtId = string.Empty,
            AtomiSignId = string.Empty,
            Email = " ",
            Offset = null,
            Limit = null,
        };

        var result = await AtomiSign.Users(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("GET"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo("/users"));
    }

    [Test]
    public async Task DeleteUserByID_ShouldAppendUserIdAndUseBasicAuthentication()
    {
        using var server = new TestHttpServer();
        var input = new Input
        {
            Method = Method.DeleteUserByID,
            UserId = UserId,
        };

        var result = await AtomiSign.Users(input, BasicConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("DELETE"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo($"/users/{UserId}"));
        Assert.That(server.Request.Headers["Authorization"], Does.StartWith("Basic "));
    }

    [Test]
    public async Task InvalidDeleteUserId_ShouldReturnFailedResult()
    {
        var input = new Input
        {
            Method = Method.DeleteUserByID,
            UserId = "not-a-guid",
        };
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await AtomiSign.Users(input, ApiTokenConnection("http://127.0.0.1:9"), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("UserId must be a valid UUID"));
    }

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
