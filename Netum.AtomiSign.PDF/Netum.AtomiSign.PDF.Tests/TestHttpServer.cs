using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Netum.AtomiSign.PDF.Tests;

// Test helper layout intentionally groups the nested request type with its server.
#pragma warning disable SA1201, SA1202
internal sealed class TestHttpServer : IDisposable
{
    internal sealed record RequestData(
        string Method,
        string PathAndQuery,
        Dictionary<string, string> Headers,
        string Body);

    private readonly TcpListener listener;
    private readonly byte[] responseBody;
    private readonly string responseContentType;
    private readonly Task acceptTask;

    internal TestHttpServer(byte[] responseBody, string responseContentType)
    {
        this.responseBody = responseBody;
        this.responseContentType = responseContentType;
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Url = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";
        acceptTask = AcceptAsync();
    }

    internal string Url { get; }

    internal RequestData Request { get; private set; }

    public void Dispose() => listener.Stop();

    internal Task WaitForRequestAsync() => acceptTask;

    private async Task AcceptAsync()
    {
        using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, false, leaveOpen: true);

        var requestLine = await reader.ReadLineAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string line;
        while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync().ConfigureAwait(false)))
        {
            var separatorIndex = line.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex > 0)
                headers[line[..separatorIndex]] = line[(separatorIndex + 1)..].Trim();
        }

        var contentLength = headers.TryGetValue("Content-Length", out var length)
            ? int.Parse(length, System.Globalization.CultureInfo.InvariantCulture)
            : 0;
        var bodyBuffer = new char[contentLength];
        if (contentLength > 0)
            await reader.ReadBlockAsync(bodyBuffer, 0, contentLength).ConfigureAwait(false);

        var requestParts = requestLine?.Split(' ') ?? [];
        Request = new RequestData(
            requestParts.Length > 0 ? requestParts[0] : string.Empty,
            requestParts.Length > 1 ? requestParts[1] : string.Empty,
            headers,
            new string(bodyBuffer));

        var responseHeaders =
            $"HTTP/1.1 200 OK\r\nContent-Type: {responseContentType}\r\nContent-Length: {responseBody.Length}\r\nConnection: close\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(responseHeaders)).ConfigureAwait(false);
        await stream.WriteAsync(responseBody).ConfigureAwait(false);
    }
}
#pragma warning restore SA1201, SA1202
