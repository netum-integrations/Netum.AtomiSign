using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Records.Definitions;
using Netum.AtomiSign.Records.Helpers;

namespace Netum.AtomiSign.Records;

/// <summary>
/// Task Class for AtomiSign operations.
/// </summary>
public static class AtomiSign
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Record task.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Netum-AtomiSign-Records)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Body, string ContentType, Dictionary Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Records(
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

        if (RequiresRecordId(input.Method) && !IsValidRecordId(input.RecordId))
            throw new ArgumentException("RecordId must be a valid UUID.");

        if (input.Method == Method.CreateRecord && string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("Name is required when Method is CreateRecord.");

        if (input.Method == Method.CreateRecord && string.IsNullOrWhiteSpace(input.Agents))
            throw new ArgumentException("Agents is required when Method is CreateRecord.");
    }

    private static HttpRequestMessage CreateRequest(Input input, Connection connection)
    {
        var request = new HttpRequestMessage(GetHttpMethod(input.Method), BuildUri(connection.BaseUrl, input));

        if (HasRequestBody(input.Method))
            request.Content = new StringContent(CreateRequestBody(input), Encoding.UTF8, "application/json");

        if (!string.IsNullOrWhiteSpace(input.Accept))
            request.Headers.TryAddWithoutValidation("Accept", input.Accept);

        AddAuthentication(request, connection);

        return request;
    }

    private static HttpMethod GetHttpMethod(Method method) => method switch
    {
        Method.CreateRecord => HttpMethod.Post,
        Method.ReturnAllRecords => HttpMethod.Get,
        Method.GetRecordByID => HttpMethod.Get,
        Method.ReplaceRecord => HttpMethod.Put,
        Method.DeleteRecord => HttpMethod.Delete,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
    };

    private static Uri BuildUri(string baseUrl, Input input)
    {
        var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
        var normalizedEndpoint = input.Endpoint.Trim('/');
        var path = RequiresRecordId(input.Method)
            ? $"{normalizedEndpoint}/{Uri.EscapeDataString(input.RecordId)}"
            : normalizedEndpoint;

        var uriBuilder = new UriBuilder(new Uri(new Uri(normalizedBaseUrl), path));

        if (input.Method == Method.ReturnAllRecords)
            uriBuilder.Query = BuildQuery(input);

        return uriBuilder.Uri;
    }

    private static string BuildQuery(Input input)
    {
        var queryParameters = BuildQueryParameters(input).ToArray();
        return string.Join("&", queryParameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildQueryParameters(Input input)
    {
        if (!string.IsNullOrWhiteSpace(input.Term))
            yield return new KeyValuePair<string, string>("term", input.Term);

        foreach (var status in input.Statuses ?? [])
        {
            if (!string.IsNullOrWhiteSpace(status))
                yield return new KeyValuePair<string, string>("statuses", status);
        }

        if (input.Own.HasValue)
            yield return new KeyValuePair<string, string>("own", input.Own.Value.ToString().ToLowerInvariant());

        if (input.UpdatedMin.HasValue)
            yield return new KeyValuePair<string, string>("updated-min", input.UpdatedMin.Value.ToString("O", CultureInfo.InvariantCulture));

        if (!string.IsNullOrWhiteSpace(input.QueryStatus))
            yield return new KeyValuePair<string, string>("status", input.QueryStatus);

        yield return new KeyValuePair<string, string>("offset", input.Offset.ToString(CultureInfo.InvariantCulture));
        yield return new KeyValuePair<string, string>("limit", input.Limit.ToString(CultureInfo.InvariantCulture));
    }

    private static string CreateRequestBody(Input input)
    {
        var requestBody = new
        {
            input.Name,
            input.Message,
            Authentication = GetRecordAuthenticationValue(input.RecordAuthentication),
            input.Status,
            input.ExpiresAt,
            input.SendRemindersAt,
            input.Metadata,
            SigningOrder = GetSigningOrderValue(input.SigningOrder),
            SignaturePage = GetSignaturePageValue(input.SignaturePage),
            Agents = NormalizeAgents(input.Agents),
        };

        return JsonSerializer.Serialize(requestBody, JsonSerializerOptions);
    }

    private static object NormalizeAgents(string agents)
    {
        if (string.IsNullOrWhiteSpace(agents))
            return null;

        var parsedAgents = JsonSerializer.Deserialize<JsonElement>(agents);

        if (parsedAgents.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Agents must be a JSON array.");

        foreach (var agent in parsedAgents.EnumerateArray())
        {
            if (agent.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Every item in Agents must be a JSON object.");
        }

        return parsedAgents;
    }

    private static string GetSigningOrderValue(SigningOrder signingOrder) => signingOrder switch
    {
        SigningOrder.Parallel => "parallel",
        SigningOrder.Sequential => "sequential",
        _ => throw new ArgumentOutOfRangeException(nameof(signingOrder), signingOrder, null),
    };

    private static string GetRecordAuthenticationValue(RecordAuthentication authentication) => authentication switch
    {
        RecordAuthentication.Light => "light",
        RecordAuthentication.Sms => "sms",
        RecordAuthentication.Strong => "strong",
        _ => throw new ArgumentOutOfRangeException(nameof(authentication), authentication, null),
    };

    private static string GetSignaturePageValue(SignaturePage signaturePage) => signaturePage switch
    {
        SignaturePage.NoPage => "no-page",
        SignaturePage.SignaturePage => "signature-page",
        SignaturePage.SignatureFields => "signature-fields",
        _ => throw new ArgumentOutOfRangeException(nameof(signaturePage), signaturePage, null),
    };

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

    private static bool RequiresRecordId(Method method) =>
        method == Method.GetRecordByID || method == Method.ReplaceRecord || method == Method.DeleteRecord;

    private static bool HasRequestBody(Method method) =>
        method == Method.CreateRecord || method == Method.ReplaceRecord;

    private static bool IsValidRecordId(string recordId) =>
        Guid.TryParse(recordId, out var parsedRecordId) && parsedRecordId != Guid.Empty;

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
