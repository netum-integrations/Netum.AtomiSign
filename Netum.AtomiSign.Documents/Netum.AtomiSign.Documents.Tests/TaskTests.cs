using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Documents.Definitions;
using NUnit.Framework;

namespace Netum.AtomiSign.Documents.Tests;

[TestFixture]
internal class TaskTests : TestBase
{
    private const string RecordId = "11111111-1111-1111-1111-111111111111";
    private const string DocumentId = "22222222-2222-2222-2222-222222222222";

    [Test]
    public async Task AddDocument_ShouldPostMultipartForm()
    {
        using var server = new TestHttpServer("[{\"id\":\"document\"}]");
        var input = new Input
        {
            Method = Method.AddDocument,
            RecordId = RecordId,
            File = Encoding.UTF8.GetBytes("hello"),
            Name = "agreement.txt",
            Order = 2,
        };

        var result = await AtomiSign.Documents(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("POST"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo($"/records/{RecordId}/documents"));
        Assert.That(server.Request.Headers["Authorization"], Is.EqualTo("Bearer token"));
        Assert.That(server.Request.Headers["Content-Type"], Does.StartWith("multipart/form-data"));
        Assert.That(server.Request.Body, Does.Contain("name=file"));
        Assert.That(server.Request.Body, Does.Contain("filename=agreement.txt"));
        Assert.That(server.Request.Body, Does.Contain("name=order"));
    }

    [Test]
    public async Task UpdateDocument_ShouldPutMultipartFormWithBasicAuthentication()
    {
        using var server = new TestHttpServer("[{\"id\":\"document\"}]");
        var input = new Input
        {
            Method = Method.UpdateDocument,
            RecordId = RecordId,
            DocumentId = DocumentId,
            File = Encoding.UTF8.GetBytes("updated"),
            Name = "updated.txt",
        };

        var result = await AtomiSign.Documents(input, BasicConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo("PUT"));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo($"/records/{RecordId}/documents/{DocumentId}"));
        Assert.That(server.Request.Headers["Authorization"], Does.StartWith("Basic "));
        Assert.That(server.Request.Body, Does.Contain("updated.txt"));
    }

    [TestCase(Method.GetDocumentByID, "GET")]
    [TestCase(Method.DeleteDocument, "DELETE")]
    public async Task DocumentIdOperations_ShouldAppendDocumentId(Method method, string expectedHttpMethod)
    {
        using var server = new TestHttpServer();
        var input = new Input
        {
            Method = method,
            RecordId = RecordId,
            DocumentId = DocumentId,
        };

        var result = await AtomiSign.Documents(input, ApiTokenConnection(server.Url), DefaultOptions(), CancellationToken.None);
        await server.WaitForRequestAsync();

        Assert.That(result.Success, Is.True);
        Assert.That(server.Request.Method, Is.EqualTo(expectedHttpMethod));
        Assert.That(server.Request.PathAndQuery, Is.EqualTo($"/records/{RecordId}/documents/{DocumentId}"));
    }

    [Test]
    public async Task AddDocumentWithoutFile_ShouldReturnFailedResult()
    {
        var input = new Input
        {
            Method = Method.AddDocument,
            RecordId = RecordId,
            File = [],
        };
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await AtomiSign.Documents(input, ApiTokenConnection("http://127.0.0.1:9"), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("File is required"));
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
