using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Documents.Definitions;
using Netum.AtomiSign.Documents.Helpers;

namespace Netum.AtomiSign.Documents;

/// <summary>
/// Task Class for AtomiSign operations.
/// </summary>
public static class AtomiSign
{
    /// <summary>
    /// Documents task.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Netum-AtomiSign-Documents)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Body, string ContentType, Dictionary Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Documents(
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
            var body = await response.Content.ReadAsStringAsync(timeoutCancellationTokenSource.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"AtomiSign API returned status code {(int)response.StatusCode}: {body}");

            return new Result
            {
                Success = true,
                Body = body,
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

        if (!IsValidId(input.RecordId))
            throw new ArgumentException("RecordId must be a valid UUID.");

        if (RequiresDocumentId(input.Method) && !IsValidId(input.DocumentId))
            throw new ArgumentException("DocumentId must be a valid UUID.");

        if (input.Method == Method.AddDocument && (input.File == null || input.File.Length == 0))
            throw new ArgumentException("File is required when Method is AddDocument.");
    }

    private static HttpRequestMessage CreateRequest(Input input, Connection connection)
    {
        var request = new HttpRequestMessage(GetHttpMethod(input.Method), BuildUri(connection.BaseUrl, input));

        if (HasRequestBody(input.Method))
            request.Content = CreateFormContent(input);

        if (!string.IsNullOrWhiteSpace(input.Accept))
            request.Headers.TryAddWithoutValidation("Accept", input.Accept);

        AddAuthentication(request, connection);

        return request;
    }

    private static HttpMethod GetHttpMethod(Method method) => method switch
    {
        Method.AddDocument => HttpMethod.Post,
        Method.UpdateDocument => HttpMethod.Put,
        Method.GetDocumentByID => HttpMethod.Get,
        Method.DeleteDocument => HttpMethod.Delete,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
    };

    private static Uri BuildUri(string baseUrl, Input input)
    {
        var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
        var normalizedEndpoint = input.Endpoint.Trim('/');
        var path = $"{normalizedEndpoint}/{Uri.EscapeDataString(input.RecordId)}/documents";

        if (RequiresDocumentId(input.Method))
            path = $"{path}/{Uri.EscapeDataString(input.DocumentId)}";

        return new Uri(new Uri(normalizedBaseUrl), path);
    }

    private static MultipartFormDataContent CreateFormContent(Input input)
    {
        var content = new MultipartFormDataContent();

        if (input.File != null && input.File.Length > 0)
        {
            var fileName = string.IsNullOrWhiteSpace(input.Name) ? "document" : input.Name;
            content.Add(new ByteArrayContent(input.File), "file", fileName);
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
            content.Add(new StringContent(input.Name), "name");

        if (input.Order.HasValue)
            content.Add(new StringContent(input.Order.Value.ToString(CultureInfo.InvariantCulture)), "order");

        return content;
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

    private static bool RequiresDocumentId(Method method) =>
        method == Method.UpdateDocument || method == Method.GetDocumentByID || method == Method.DeleteDocument;

    private static bool HasRequestBody(Method method) =>
        method == Method.AddDocument || method == Method.UpdateDocument;

    private static bool IsValidId(string id) =>
        Guid.TryParse(id, out var parsedId) && parsedId != Guid.Empty;

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
