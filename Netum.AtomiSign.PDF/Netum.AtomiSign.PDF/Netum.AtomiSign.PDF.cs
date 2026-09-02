using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.PDF.Definitions;
using Netum.AtomiSign.PDF.Helpers;

namespace Netum.AtomiSign.PDF;

/// <summary>
/// Task Class for AtomiSign operations.
/// </summary>
public static class AtomiSign
{
    /// <summary>
    /// Converts the given text content to PDF/A.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Netum-AtomiSign-PDF)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, byte[] BodyBytes, double BodySizeInMegaBytes, string ContentType, Dictionary Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> PDF(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);
            ValidateOperation(input, connection, options);
            cancellationToken.ThrowIfCancellationRequested();

            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

            using var httpClient = new HttpClient();
            using var request = CreateRequest(input, connection);
            using var response = await SendAsync(httpClient, request, timeoutCancellationTokenSource.Token).ConfigureAwait(false);
            var bodyBytes = await response.Content.ReadAsByteArrayAsync(timeoutCancellationTokenSource.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"AtomiSign API returned status code {(int)response.StatusCode}: {Encoding.UTF8.GetString(bodyBytes)}");

            return new Result
            {
                Success = true,
                BodyBytes = bodyBytes,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty,
                Headers = GetResponseHeaders(response),
                StatusCode = (int)response.StatusCode,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static void ValidateOperation(Input input, Connection connection, Options options)
    {
        if (options.TimeoutSeconds <= 0)
            throw new ArgumentException("TimeoutSeconds must be greater than zero.");

        if (connection.Authentication == AuthenticationMethod.ApiToken && string.IsNullOrWhiteSpace(connection.ApiToken))
            throw new ArgumentException("ApiToken is required when Authentication is ApiToken.");

        if (connection.Authentication == AuthenticationMethod.Basic &&
            (string.IsNullOrWhiteSpace(connection.Username) || string.IsNullOrWhiteSpace(connection.Password)))
            throw new ArgumentException("Username and Password are required when Authentication is Basic.");

        if (string.IsNullOrWhiteSpace(input.Content))
            throw new ArgumentException("Content is required.");
    }

    private static HttpRequestMessage CreateRequest(Input input, Connection connection)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(connection.BaseUrl, input.Endpoint))
        {
            Content = CreateFormContent(input),
        };

        if (!string.IsNullOrWhiteSpace(input.Accept))
            request.Headers.TryAddWithoutValidation("Accept", input.Accept);

        AddAuthentication(request, connection);

        return request;
    }

    private static MultipartFormDataContent CreateFormContent(Input input)
    {
        var content = new MultipartFormDataContent();
        var pdfContent = new ByteArrayContent(CreatePdfBytes(input.Content, input.Title));
        pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(pdfContent, "pdf", "document.pdf");

        if (!string.IsNullOrWhiteSpace(input.Title))
            content.Add(new StringContent(input.Title), "title");

        return content;
    }

    private static byte[] CreatePdfBytes(string text, string title)
    {
        var escapedText = EscapePdfString(text);
        var escapedTitle = EscapePdfString(string.IsNullOrWhiteSpace(title) ? "Converted document" : title);

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount($"BT /F1 12 Tf 50 780 Td ({escapedText}) Tj ET")} >>\nstream\nBT /F1 12 Tf 50 780 Td ({escapedText}) Tj ET\nendstream",
            $"<< /Title ({escapedTitle}) >>",
        };

        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };

        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n0 ").Append(objects.Length + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");

        for (var i = 1; i < offsets.Count; i++)
            builder.Append(offsets[i].ToString("D10", System.Globalization.CultureInfo.InvariantCulture)).Append(" 00000 n \n");

        builder.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R /Info 6 0 R >>\n");
        builder.Append("startxref\n").Append(xrefOffset).Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdfString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal)
            .Replace("\r\n", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\n", StringComparison.Ordinal);
    }

    private static Uri BuildUri(string baseUrl, string endpoint)
    {
        var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
        var normalizedEndpoint = endpoint.TrimStart('/');
        return new Uri(new Uri(normalizedBaseUrl), normalizedEndpoint);
    }

    private static void AddAuthentication(HttpRequestMessage request, Connection connection)
    {
        switch (connection.Authentication)
        {
            case AuthenticationMethod.ApiToken:
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {connection.ApiToken}");
                break;
            case AuthenticationMethod.Basic:
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connection.Username}:{connection.Password}"));
                request.Headers.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connection.Authentication), connection.Authentication, null);
        }
    }

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException exception)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new TimeoutException("HTTP request was canceled.", exception);

            throw;
        }
    }

    private static Dictionary<string, string> GetResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in response.Headers)
            headers[header.Key] = string.Join(";", header.Value);

        foreach (var header in response.Content.Headers)
            headers[header.Key] = string.Join(";", header.Value);

        return headers;
    }
}
