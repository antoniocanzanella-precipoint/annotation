using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.DataTransfer.Http;
using PreciPoint.Ims.Core.DataTransfer.Mapping;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Core.DataTransferObjects.Users;
using PreciPoint.Ims.Core.IdentityModel;
using PreciPoint.Ims.Core.IdentityModel.Factories;
using PreciPoint.Ims.Core.IdentityModel.Tokens;
using PreciPoint.Ims.Services.Annotation.MessagePack.Resolver;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation;

/// <summary>
/// This is a modified version of the <see cref="IdentityHttpClient"/>. It includes <see cref="GetMessagePackAsync{T}" /> to allow for a
/// get request with messagepack content.
/// </summary>
public class MessagePackIdentityHttpClient : AHttpClient
{
    private readonly MessagePackSerializerOptions _messagePackSerializerOptions;
    private readonly ITokenService _tokenService;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly OidcHttpClient _oidcHttpClient;

    /// <summary>
    /// Creates a new MessagePackIdentityHttpClient client.
    /// </summary>
    /// <param name="tokenService">The token service to use.</param>
    /// <param name="userInfoMapper">The userInfoMapper to use.</param>
    /// <param name="messagePackSerializerOptions">Optional serialization options for message pack.</param>
    /// <param name="jsonSerializerOptions">Optional serialization options for json.</param>
    /// <param name="loggerFactory">Optional logger factory.</param>
    /// <param name="httpClient">Optional http client that can be used to overwrite.</param>
    /// <param name="httpContentFactory">Optional httpContentFactory.</param>
    public MessagePackIdentityHttpClient(ITokenService tokenService, IUserInfoMapper userInfoMapper, MessagePackSerializerOptions messagePackSerializerOptions = null,
        JsonSerializerOptions jsonSerializerOptions = null,
        ILoggerFactory loggerFactory = null, HttpClient httpClient = null,
        HttpContentFactory httpContentFactory = null)
        : base(loggerFactory, jsonSerializerOptions, httpClient, httpContentFactory)
    {
        _tokenService = tokenService;
        _userInfoMapper = userInfoMapper;
        if (messagePackSerializerOptions is null)
        {
            IFormatterResolver resolver = CompositeResolver.Create(
                AnnotationDtoFormatterResolver.Instance,
                ContractlessStandardResolver.Instance
            );
            MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            _messagePackSerializerOptions = options;
        }
        else
        {
            _messagePackSerializerOptions = messagePackSerializerOptions;
        }

        _oidcHttpClient = new OidcHttpClient(_tokenService, loggerFactory, JsonSerializerOptions, HttpClient, HttpContentFactory);
    }

    /// <inheritdoc/>
    public override async Task<HttpResponseMessage> SendHttpContentAsync(string url, HttpMethod httpMethod, HttpContent httpContent = null,
        string accessToken = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _oidcHttpClient.SendHttpContentAsync(url, httpMethod, httpContent, accessToken, cancellationToken);

        return response;
    }

    /// <summary>
    /// Considers the given token request to explicitly ask backend for current user information.
    /// </summary>
    /// <param name="cancellationToken">Allows to cancel eventually ongoing requests.</param>
    /// <returns>The user object describing the current OAuth user.</returns>
    public override async Task<ApiResponse<UserInfoDto>> RetrieveUserInfo(
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("We'll try to retrieve user info from URL '{url}'.", _tokenService.UserInfoUrl);
        string accessToken = await _tokenService.RetrieveAccessToken(cancellationToken);

        if (accessToken is null)
        {
            throw new UnauthorizedAccessException("Unable to retrieve access token.");
        }

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _tokenService.UserInfoUrl)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
        };
        using HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Unable to read user information related response correctly.");
        }

        UserInfoDto userInfo = await _userInfoMapper.MapFromStream(stream);

        return new ApiResponse<UserInfoDto>(userInfo);
    }

    /// <summary>
    /// Clients itself could be interested in the current access token.
    /// </summary>
    /// <param name="cancellationToken">Allows to cancel the ongoing token retrieval request.</param>
    /// <returns>The access token that is currently in use.</returns>
    public override Task<string> RetrieveAccessToken(CancellationToken cancellationToken = default)
    {
        return _tokenService.RetrieveAccessToken(cancellationToken);
    }

    /// <summary>
    /// Ends the corresponding user session. Be aware, that if you logout a session, all HTTP API clients that made use of it won't be able to use it
    /// anymore. This fact is quite relevant as usually there should only be one shared single instance of this class that represents a user session. 
    /// </summary>
    /// <param name="cancellationToken">Allows to cancel the ongoing logout request.</param>
    /// <returns>The task that handles the logout.</returns>
    public override Task LogoutAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return _tokenService.Logout(cancellationToken);
    }

    /// <summary>
    /// Disposes the underlying HTTP client and all other relevant resources.
    /// </summary>
    public override void Dispose()
    {
        _oidcHttpClient.Dispose();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override UserInfoDto UserInfo => _userInfoMapper.MapFromAccessToken(_tokenService.AccessToken);

    /// <summary>
    /// Sends a http GET request and tries to deserialize the http body with message pack
    /// </summary>
    /// <param name="url">The url to hit</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <returns>Task resulting in T</returns>
    public async Task<T> GetMessagePackAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        string accessToken = await _tokenService.RetrieveAccessToken(cancellationToken);
        HttpResponseMessage result = await SendHttpContentAsync(url, HttpMethod.Get, null, accessToken, cancellationToken);
        Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken);

        return await MessagePackSerializer.DeserializeAsync<T>(stream, _messagePackSerializerOptions,
            cancellationToken);
    }
}