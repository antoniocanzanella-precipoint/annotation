using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database;

public class AnnotationDbContext : DbContext, IDbContext
{
    public const string DefaultSchema = "ims";

    static AnnotationDbContext()
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<AnnotationType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<AnnotationVisibility>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<AnnotationPermission>();
    }

    public AnnotationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

    public AnnotationDbContext() { }

    public virtual DbSet<AnnotationShape> Annotations { get; set; }
    public virtual DbSet<Counter> Counters { get; set; }
    public virtual DbSet<CounterGroup> CounterGroups { get; set; }
    public virtual DbSet<Folder> Folders { get; set; }

    public DatabaseFacade GetDatabase()
    {
        return base.Database;
    }

    public void Entry<TEntity>(TEntity entity, EntityState state) where TEntity : class
    {
        base.Entry(entity).State = state;
    }

    public Task<int> ExecuteSqlRawAsync(string sqlRaw, CancellationToken cancellationToken)
    {
        return base.Database.ExecuteSqlRawAsync(sqlRaw, cancellationToken);
    }

    public IQueryable<Folder> GetFolderBelow(IRawQueryResolver rawQueryResolver, Guid folderId)
    {
        return base.Set<Folder>().FromSqlRaw(rawQueryResolver.GetFolderBelowQuery(), folderId);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasPostgresEnum<AnnotationType>();
        modelBuilder.HasPostgresEnum<AnnotationVisibility>();
        modelBuilder.HasPostgresEnum<AnnotationPermission>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnnotationDbContext).Assembly);
    }
}