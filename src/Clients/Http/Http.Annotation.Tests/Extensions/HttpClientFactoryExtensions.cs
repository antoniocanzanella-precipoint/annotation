using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Core.DataTransfer.Config;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Core.DataTransfer.Http;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;

internal static class HttpClientFactoryExtensions
{
    public static IdentityHttpClient CreateUserHttpClient(this HttpClientFactory httpClientFactory, AnnotationTestConfig annotationTestConfig,
        ILoggerFactory loggerFactory = null)
    {
        return httpClientFactory.CreateIdentityHttpClientFromPasswordTokenRequestUser(
            new PasswordTokenRequestUser
            {
                UserName = annotationTestConfig.User01.UserName,
                Password = annotationTestConfig.User01.Password,
                ClientId = annotationTestConfig.User01.ClientId
            }, null, null, null, loggerFactory
        );
    }

    public static IdentityHttpClient CreateUser2HttpClient(this HttpClientFactory httpClientFactory, AnnotationTestConfig annotationTestConfig,
        ILoggerFactory loggerFactory = null)
    {
        return httpClientFactory.CreateIdentityHttpClientFromPasswordTokenRequestUser(
            new PasswordTokenRequestUser
            {
                UserName = annotationTestConfig.User02.UserName,
                Password = annotationTestConfig.User02.Password,
                ClientId = annotationTestConfig.User02.ClientId
            }, null, null, null, loggerFactory
        );
    }

    public static IdentityHttpClient CreateAdminHttpClient(this HttpClientFactory httpClientFactory, AnnotationTestConfig annotationTestConfig,
        ILoggerFactory loggerFactory = null)
    {
        return httpClientFactory.CreateIdentityHttpClientFromPasswordTokenRequestUser(
            new PasswordTokenRequestUser
            {
                UserName = annotationTestConfig.Admin.UserName,
                Password = annotationTestConfig.Admin.Password,
                ClientId = annotationTestConfig.Admin.ClientId
            }, null, null, null, loggerFactory
        );
    }
}