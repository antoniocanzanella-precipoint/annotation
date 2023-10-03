using System;

namespace PreciPoint.Ims.Services.Annotation.Domain;

public static class DomainConstants
{
    #region Import

    public const int FileNameLenght = 100;

    #endregion

    public static readonly Guid AnonymousId = new("00000000-0000-0000-0000-000000000000");

    #region Annotation Entity

    public const int AnnotationLabelLength = 128;
    public const int AnnotationDescriptionLength = 1024;

    #endregion

    #region Group Counter Entity

    public const int GroupCounterLabelLength = 128;
    public const int GroupCounterDescriptionLength = 1024;

    #endregion

    #region Folders

    public const int FolderNameLength = 64;
    public const int FolderBriefDescriptionLength = 512;
    public const int FolderDescriptionLength = 1024;

    #endregion
}