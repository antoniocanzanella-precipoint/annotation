using PreciPoint.Ims.Services.Annotation.Application.Interfaces;

namespace PreciPoint.Ims.Services.Annotation.Database.Queries;

public class RawQueryResolver : IRawQueryResolver
{
    public string GetFolderBelowQuery()
    {
        return
            @"WITH recursive folders (""Id"", ""Name"", ""BriefDescription"", ""Description"", ""ParentFolderId"", ""DisplayOder"", FolderBelow) AS (" +
            @"SELECT f.""Id"", f.""Name"", f.""BriefDescription"", f.""Description"", f.""ParentFolderId"", f.""DisplayOder"", 0 FROM ims.""Folders"" f WHERE f.""Id"" = {0} " +
            @"UNION ALL " +
            @"SELECT e.""Id"", e.""Name"", e.""BriefDescription"", e.""Description"", e.""ParentFolderId"", e.""DisplayOder"", o.FolderBelow + 1 FROM ims.""Folders"" e INNER JOIN folders o ON o.""Id"" = e.""ParentFolderId"" " +
            @") SELECT * FROM folders";
    }
}