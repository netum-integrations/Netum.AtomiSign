using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Signatures.Definitions;
using Netum.AtomiSign.Signatures.Helpers;

namespace Netum.AtomiSign.Signatures;

/// <summary>
/// Task Class for AtomiSign operations.
/// </summary>
public static class AtomiSign
{
    /// <summary>
    /// Signs the given file and returns the signed file.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Netum-AtomiSign-Signatures)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, byte[] BodyBytes, double BodySizeInMegaBytes, string ContentType, Dictionary Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Signatures(
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

        if (input.File == null || input.File.Length == 0)
            throw new ArgumentException("File is required.");
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
        content.Add(new ByteArrayContent(input.File), "file", "document.pdf");
        content.Add(new StringContent(input.SignatureLevel.ToString()), "signatureLevel");
        return content;
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
