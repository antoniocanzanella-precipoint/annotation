using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

public class Import : AEntityAuditable
{
    public string FileName { get; set; }
    public byte[] File { get; set; }
    public ICollection<AnnotationShape> Annotations { get; set; }
}