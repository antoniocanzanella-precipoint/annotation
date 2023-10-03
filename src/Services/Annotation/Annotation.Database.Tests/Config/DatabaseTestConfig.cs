using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Config;

public class DatabaseTestConfig
{
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

    public string ConnectionString { get; set; }
}