using NUnit.Framework;
using PreciPoint.Ims.Core.HealthCheck.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(13)]
[NonParallelizable]
[Category("Integration")]
internal class I013HealthCheck : ABaseTest
{
    [Test]
    [Order(1)]
    public async Task I013_001VerifyHealthy()
    {
        var uri = new Uri(Configuration.AnnotationHost);
        using var httpClient = new HttpClient { BaseAddress = HealthCheckExtensions.GetManagementUri(uri.Host, uri.Port) };

        HttpResponseMessage response = await httpClient.GetAsync("/health/ready", CancellationToken.None);
        httpClient.Dispose();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}