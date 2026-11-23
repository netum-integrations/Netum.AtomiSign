using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.PDF.Definitions;
using NUnit.Framework;

namespace Netum.AtomiSign.PDF.Tests;

[TestFixture]
internal class TaskTests : TestBase
{
    [Test]
    public async Task ShouldReturnSuccessfulResultForValidRequestWithBasicAuthentication()
    {
        var expectedBody = Encoding.ASCII.GetBytes("%PDF-1.4\nmock converted document");
        using var server = new TestHttpServer(expectedBody, "application/pdf");
        var input = new Input
        {
            Content = "Text to convert",
            Title = "Converted document",
        };
        var connection = new Connection
        {
            BaseUrl = server.Url,
            Authentication = AuthenticationMethod.Basic,
            Username = "user",
            Password = "pass",
        };

        var result = await AtomiSign.PDF(input, connection, DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.ContentType, Does.StartWith("application/pdf"));
        Assert.That(result.BodyBytes, Is.EqualTo(expectedBody));
        Assert.That(result.Error, Is.Null);
        Assert.That(server.Request.Method, Is.EqualTo("POST"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo("/pdf/convert"));
        Assert.That(server.Request.Headers["Authorization"], Does.StartWith("Basic "));
        Assert.That(server.Request.Headers["Content-Type"], Does.StartWith("multipart/form-data"));
        Assert.That(server.Request.Body, Does.Contain("name=pdf"));
        Assert.That(server.Request.Body, Does.Contain("filename=document.pdf"));
    }

    [Test]
    public async Task ShouldReturnFailedResultWhenRequestFailsAndThrowErrorOnFailureIsFalse()
    {
        var input = new Input
        {
            Content = "Text to convert",
            Title = "Converted document",
        };

        var connection = new Connection
        {
            BaseUrl = "http://127.0.0.1:9",
            Authentication = AuthenticationMethod.ApiToken,
            ApiToken = "token",
        };

        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = null,
            TimeoutSeconds = 1,
        };

        var result = await AtomiSign.PDF(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }
}
