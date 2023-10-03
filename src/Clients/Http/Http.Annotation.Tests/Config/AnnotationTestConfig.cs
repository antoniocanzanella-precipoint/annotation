using PreciPoint.Ims.Core.DataTransfer.Config;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;

/// <summary>
/// the configuration for test project
/// </summary>
public class AnnotationTestConfig
{
    /// <summary>
    /// We need fixed guid to share data into benchmark job. this must follow the directory structure.
    /// </summary>
    public IList<Guid> SlideImageIds = new List<Guid>
    {
        new("f56c73bd-be98-41f9-94b5-9ba1f4e098e1"), //this is the most expensive
        new("58740589-d449-4a92-bab9-76da922d034f"),
        new("8f3b7231-4d19-4fd2-bf21-49de2699d1f2"),
        new("81dbce2a-d132-4226-ac6c-2c6f62e038fa"),
        new("eb60ac27-7c8f-49fb-85de-e32f136172b7"),
        new("6d2a9098-ceb8-4d0a-94d0-1206c15737d9"),
        new("d04b069d-46ad-42e3-8089-59829cc9dbf4")
    };

    /// <summary>
    /// The URL a signaling hub is listening on.
    /// </summary>
    public string AnnotationUrlSignalR { get; set; }

    /// <summary>
    /// The hostname of the annotation service we want to run test against.
    /// </summary>
    public string AnnotationHost { get; set; }

    /// <summary>
    /// The hostname of the image management service we want to run test against.
    /// </summary>
    public string ImageManagementHost { get; set; }

    /// <summary>
    /// The client access path root of the slide storage we store test slide images in.
    /// </summary>
    public string ClientAccessPathRoot { get; set; }

    /// <summary>
    /// fonder containing the slide images to upload
    /// </summary>
    public string UploadFolder { get; set; }

    /// <summary>
    /// The amount of parallel running HTTP clients.
    /// </summary>
    public int ParallelClients { get; set; }

    /// <summary>
    /// The amount of annotations for a single slide image.
    /// </summary>
    public int NumberOfAnnotationPerSlideImage { get; set; }

    /// <summary>
    /// The security system configuration that applies for tested hosts.
    /// </summary>
    public SecuritySystemConfig SecuritySystem { get; set; }

    /// <summary>
    /// Annotation user
    /// </summary>
    public PasswordTokenRequestUser User01 { get; set; }

    /// <summary>
    /// Annotation user
    /// </summary>
    public PasswordTokenRequestUser User02 { get; set; }

    /// <summary>
    /// Annotation Admin
    /// </summary>
    public PasswordTokenRequestUser Admin { get; set; }

    public Guid GetExpensiveSlideImage()
    {
        return SlideImageIds[0];
    }
}