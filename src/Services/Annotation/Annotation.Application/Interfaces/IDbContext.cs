using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Interfaces;

public interface IDbContext
{
    DbSet<T> Set<T>() where T : class;
    DatabaseFacade GetDatabase();
    void Entry<TEntity>(TEntity entity, EntityState state) where TEntity : class;
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> ExecuteSqlRawAsync(string sqlRaw, CancellationToken cancellationToken);
    IQueryable<Folder> GetFolderBelow(IRawQueryResolver rawQueryResolver, Guid folderId);
}