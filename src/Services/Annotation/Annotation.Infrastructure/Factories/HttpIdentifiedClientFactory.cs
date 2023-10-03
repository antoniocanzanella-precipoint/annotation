using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Core.DataTransfer.Http;
using PreciPoint.Ims.Core.IdentityModel.Tokens;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Domain.Configuration;
using System;
using System.Collections.Concurrent;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Factories;

internal class HttpIdentifiedClientFactory : IHttpClientFactory
{
    private readonly ApplicationConfig _applicationConfig;
    private readonly ConcurrentDictionary<int, Lazy<ImageManagementHttpClient>> _imageManagementHttpClients;
    private readonly ILogger<HttpIdentifiedClientFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public HttpIdentifiedClientFactory(ApplicationConfig applicationConfig, ILoggerFactory loggerFactory)
    {
        _applicationConfig = applicationConfig;
        _logger = loggerFactory.CreateLogger<HttpIdentifiedClientFactory>();
        _loggerFactory = loggerFactory;
        _imageManagementHttpClients = new ConcurrentDictionary<int, Lazy<ImageManagementHttpClient>>();
    }

    public ImageManagementHttpClient GetOrAddImageManagementHttpClient(int clientIndex = 0)
    {
        return _imageManagementHttpClients.GetOrAdd(clientIndex, _ => new Lazy<ImageManagementHttpClient>(() =>
        {
            var tokenService = new TokenRequestService(new ClientCredentialsTokenRequest
            {
                ClientId = _applicationConfig.OAuth2.Jwt.ClientId,
                ClientSecret = _applicationConfig.OAuth2.Jwt.ClientSecret,
                Address = _applicationConfig.OAuth2.TokenUrl
            }, null, null, _loggerFactory);

            string apiUrl = ExtractApiUrl(_applicationConfig.ImageManagement);

            _logger.LogInformation("Creation of image management responsible HTTP client for '{apiUrl}' successful.", apiUrl);

            return new ImageManagementHttpClient(new IdentityHttpClient(tokenService, null, _loggerFactory), apiUrl);
        })).Value;
    }

    private string ExtractApiUrl(HttpClientConfig httpClientConfig)
    {
        if (httpClientConfig == null)
        {
            return null;
        }

        string hostAndPort = HasProtocolDefaultPort(httpClientConfig.Host, httpClientConfig.Port)
            ? httpClientConfig.Host
            : $"{httpClientConfig.Host}:{httpClientConfig.Port}";

        return $"{httpClientConfig.Protocol}://{hostAndPort}/api";
    }

    private bool HasProtocolDefaultPort(string protocol, int port)
    {
        return (protocol == "http" && port == 80) || (protocol == "https" && port == 443);
    }
}