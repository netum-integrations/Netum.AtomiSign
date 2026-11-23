using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netum.AtomiSign.Users.Definitions;
using Netum.AtomiSign.Users.Helpers;

namespace Netum.AtomiSign.Users;

/// <summary>
/// Task Class for AtomiSign operations.
/// </summary>
public static class AtomiSign
{
    /// <summary>
    /// Users task.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Netum-AtomiSign-Users)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Body, string ContentType, Dictionary Headers, int StatusCode, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Users(
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

        if (!string.IsNullOrWhiteSpace(input.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(input.Email.Trim()))
            throw new ArgumentException("Email must be a valid email address.");

        if (input.Method == Method.DeleteUserByID && !IsValidId(input.UserId))
            throw new ArgumentException("UserId must be a valid UUID.");
    }

    private static HttpRequestMessage CreateRequest(Input input, Connection connection)
    {
        var request = new HttpRequestMessage(GetHttpMethod(input.Method), BuildUri(connection.BaseUrl, input));

        if (!string.IsNullOrWhiteSpace(input.Accept))
            request.Headers.TryAddWithoutValidation("Accept", input.Accept);

        AddAuthentication(request, connection);

        return request;
    }

    private static HttpMethod GetHttpMethod(Method method) => method switch
    {
        Method.GetUsers => HttpMethod.Get,
        Method.DeleteUserByID => HttpMethod.Delete,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
    };

    private static Uri BuildUri(string baseUrl, Input input)
    {
        var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
        var normalizedEndpoint = input.Endpoint.Trim('/');
        var path = input.Method == Method.DeleteUserByID
            ? $"{normalizedEndpoint}/{Uri.EscapeDataString(input.UserId)}"
            : normalizedEndpoint;
        var uriBuilder = new UriBuilder(new Uri(new Uri(normalizedBaseUrl), path));

        if (input.Method == Method.GetUsers)
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
        if (!string.IsNullOrWhiteSpace(input.ExtId))
            yield return new KeyValuePair<string, string>("extId", input.ExtId.Trim());

        if (!string.IsNullOrWhiteSpace(input.AtomiSignId))
            yield return new KeyValuePair<string, string>("atomiSignId", input.AtomiSignId.Trim());

        if (!string.IsNullOrWhiteSpace(input.Email))
            yield return new KeyValuePair<string, string>("email", input.Email.Trim());

        if (input.Offset.HasValue)
            yield return new KeyValuePair<string, string>("offset", input.Offset.Value.ToString(CultureInfo.InvariantCulture));

        if (input.Limit.HasValue)
            yield return new KeyValuePair<string, string>("limit", input.Limit.Value.ToString(CultureInfo.InvariantCulture));
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
