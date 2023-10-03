using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.Upload;
using System;
using System.Net.Http;

namespace PreciPoint.Ims.Services.Annotation.API.Form;

/// <summary>
/// Describe how <see cref="MultipartFormDataContent" /> must be structured to allows the upload
/// </summary>
public class VpaFileForm
{
    /// <summary>
    /// Representation of vpa file sent with an HTTP request.
    /// </summary>
    [FromForm(Name = VpaFileFormKeys.VpaFile)]
    public IFormFile VpaFile { get; set; }

    /// <summary>
    /// the file name
    /// </summary>
    [FromForm(Name = VpaFileFormKeys.FileName)]
    public string FileName { get; set; }

    /// <summary>
    /// the slide image unique identifier
    /// </summary>
    [FromForm(Name = VpaFileFormKeys.SlideImageId)]
    public Guid SlideImageId { get; set; }

    /// <summary>
    /// The flag indicates whether we need to respond with only the annotations uploaded
    /// or all annotations of the slide image an upload occured towards.
    /// </summary>
    [FromForm(Name = VpaFileFormKeys.ReloadAllAnnotations)]
    public bool ReloadAllAnnotations { get; set; }
}