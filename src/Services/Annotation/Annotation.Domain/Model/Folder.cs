using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

//we are using CTE common table expression
//https://www.databasestar.com/sql-cte-with/
public class Folder : AEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string BriefDescription { get; set; }
    public int DisplayOder { get; set; }
    public Guid? ParentFolderId { get; set; }
    public Folder ParentFolder { get; set; }
    public List<Folder> SubFolders { get; set; }
    public ICollection<AnnotationShape> Annotations { get; set; }
}