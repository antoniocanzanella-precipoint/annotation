using NUnit.Framework;
using PreciPoint.Ims.Core.DataTransferObjects.Exceptions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using System;
using System.Net;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(10)]
[NonParallelizable]
[Category("Integration")]
public class I010AnonymousClient : ABaseTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _apiUrl = $"{Configuration.AnnotationHost}/api";
        _annotationHttpClient = new AnnotationHttpClient(HttpClientFactory.CreateAnonymousHttpClient(), _apiUrl);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _annotationHttpClient.Dispose();
    }

    private string _apiUrl;
    private AnnotationHttpClient _annotationHttpClient;

    [Test]
    [Order(1)]
    public void I010_001TestAllowedEndPoint()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetAnnotations(new Guid()));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetAnnotationById(new Guid()));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetCounterGroups(new Guid()));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetCounterGroupById(new Guid()));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetAnnotationPermissions(new Guid()));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);
    }

    [Test]
    [Order(2)]
    public void I010_002TestForbiddenEndPoint()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.InsertAnnotation(new AnnotationDto(), new Guid()));
        Assert.AreEqual(HttpStatusCode.Forbidden, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.UpdateAnnotation(new AnnotationDto(), new Guid()));
        Assert.AreEqual(HttpStatusCode.Forbidden, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient.AnnotationClient.GetFolders(new Guid()));
        Assert.AreEqual(HttpStatusCode.Forbidden, ex.HttpStatusCode);
    }
}