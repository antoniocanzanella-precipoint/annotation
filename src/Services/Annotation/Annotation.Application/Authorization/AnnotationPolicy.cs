namespace PreciPoint.Ims.Services.Annotation.Application.Authorization;

public class AnnotationPolicy
{
    public const string ViewAnnotations = "ViewAnnotations";
    public const string ViewFolders = "ViewFolders";
    public const string ManageAnnotations = "ManageAnnotations";
    public const string ManageForeignAnnotations = "ManageForeignAnnotations";
    public const string ManageAnnotationsByFolders = "ManageAnnotationsByFolders";
    public const string DeleteAnnotations = "DeleteAnnotations";
    public const string DeleteForeignAnnotations = "DeleteForeignAnnotations";
    public const string DeleteFolders = "DeleteFolders";
    public const string ManageImport = "ManageImport";
    public const string AdminSynchronization = "AdminSynchronization";
}