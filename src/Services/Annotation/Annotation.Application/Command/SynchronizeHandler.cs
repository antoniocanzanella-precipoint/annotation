using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class Synchronize : IRequest<GenericCudOperationDto> { }

public class SynchronizeHandler : IRequestHandler<Synchronize, GenericCudOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ILogger<SynchronizeHandler> _logger;
    private readonly ISlideImageRepo _slideImageRepo;
    private readonly IStringLocalizer _stringLocalizer;

    public SynchronizeHandler(IDbContext annotationDbContext, ILogger<SynchronizeHandler> logger, ISlideImageRepo slideImageRepo,
        IStringLocalizer stringLocalizer)
    {
        _annotationDbContext = annotationDbContext;
        _logger = logger;
        _slideImageRepo = slideImageRepo;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<GenericCudOperationDto> Handle(Synchronize request, CancellationToken cancellationToken)
    {
        ApiPagedResponse<SlideImageDto> slideImageResponse;
        var slideImageIdsFromRepo = new List<Guid>();
        List<Guid> slideImageIdsIntoDb = await _annotationDbContext.Set<SlideImage>().Select(e => e.SlideImageId)
            .ToListAsync(cancellationToken);
        var pageCounter = 0;

        await using IDbContextTransaction transaction =
            await _annotationDbContext.GetDatabase().BeginTransactionAsync(cancellationToken);
        try
        {
            do
            {
                slideImageResponse = await _slideImageRepo.GetAllSlideImages(pageCounter++, cancellationToken);

                slideImageIdsFromRepo.AddRange(slideImageResponse.Data.Select(e => e.Id).ToList());

                List<SlideImageDto> slideImagesToInsert =
                    slideImageResponse.Data.Where(e => !slideImageIdsIntoDb.Contains(e.Id)).ToList();
                List<SlideImageDto> slideImagesToUpdate =
                    slideImageResponse.Data.Where(e => slideImageIdsIntoDb.Contains(e.Id)).ToList();

                //insert
                // TODO-AC: Don't do manual mapping but use AutoMapper instead with DisableConstructorMapping() on DependencyInjection
                // https://github.com/AutoMapper/AutoMapper/discussions/3862
                // services.AddAutoMapper(configAction => configAction.DisableConstructorMapping(), typeof(AutoMapperProfile).GetTypeInfo().Assembly);
                _annotationDbContext.Set<SlideImage>().AddRange(slideImagesToInsert
                    .Select(slideImageToInsert => new SlideImage(slideImageToInsert.Id, slideImageToInsert.OwnedBy)));

                //update
                Update(slideImagesToUpdate.Select(slideImageToUpdate => new SlideImage(slideImageToUpdate.Id, slideImageToUpdate.OwnedBy)).ToList());

                await _annotationDbContext.SaveChangesAsync(cancellationToken);

                if (slideImagesToInsert.Count > 0)
                {
                    _logger.LogInformation(
                        $"Inserted {slideImagesToInsert.Count} slide images from image management repository");
                }

                if (slideImagesToUpdate.Count > 0)
                {
                    _logger.LogInformation(
                        $"Updated {slideImagesToUpdate.Count} slide images from image management repository");
                }
            } while (slideImageResponse.HasNextPage);

            //REMOVE not in the list
            List<Guid> slideImageIdsToRemove = slideImageIdsIntoDb.Where(e => !slideImageIdsFromRepo.Contains(e)).ToList();
            List<SlideImage> slideImagesToRemove = await _annotationDbContext.Set<SlideImage>()
                .Where(e => slideImageIdsToRemove.Contains(e.SlideImageId)).ToListAsync(cancellationToken);

            _annotationDbContext.Set<SlideImage>().RemoveRange(slideImagesToRemove);
            await _annotationDbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (slideImagesToRemove.Count > 0)
            {
                _logger.LogInformation($"Deleted {slideImagesToRemove.Count} slide images.");
            }

            return new GenericCudOperationDto(slideImageResponse.Data.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Error during SYNCHRONIZATION. Transaction was rolled back.");
            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.SYNCHRONIZE"];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.InternalServerError);
        }
    }

    private void Update(IReadOnlyList<SlideImage> slideImages)
    {
        foreach (SlideImage slideImage in slideImages)
        {
            SlideImage slideImageToUpdate = _annotationDbContext.Set<SlideImage>()
                .FirstOrDefault(e => e.SlideImageId == slideImage.SlideImageId);

            if (slideImageToUpdate != null)
            {
                slideImageToUpdate.Update(slideImage.OwnedBy);
                _annotationDbContext.Set<SlideImage>().Update(slideImageToUpdate);
            }
        }
    }
}