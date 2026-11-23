using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Netum.AtomiSign.Records.Tests;

#pragma warning disable SA1201, SA1202
internal sealed class TestHttpServer : IDisposable
{
    internal sealed record RequestData(
        string Method,
        string PathAndQuery,
        Dictionary<string, string> Headers,
        string Body);

    private readonly TcpListener listener;
    private readonly string responseBody;
    private readonly int statusCode;
    private readonly Task acceptTask;

    public void Dispose() => listener.Stop();

    internal TestHttpServer(string responseBody = "{\"ok\":true}", int statusCode = 200)
    {
        this.responseBody = responseBody;
        this.statusCode = statusCode;
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Url = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";
        acceptTask = AcceptAsync();
    }

    internal string Url { get; }

    internal RequestData Request { get; private set; }

    internal async Task WaitForRequestAsync() => await acceptTask.ConfigureAwait(false);

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

        var responseBytes = Encoding.UTF8.GetBytes(responseBody);
        var responseHeaders = $"HTTP/1.1 {statusCode} OK\r\nContent-Type: application/json\r\nContent-Length: {responseBytes.Length}\r\nConnection: close\r\n\r\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(responseHeaders)).ConfigureAwait(false);
        await stream.WriteAsync(responseBytes).ConfigureAwait(false);
    }
}
#pragma warning restore SA1201, SA1202
