using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Attribute;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Extensions;
using PreciPoint.Ims.Services.Annotation.Domain.Helper;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.Extensions;

public static class DeckGlLayerExtensions
{
    private static AttributeHeaderDto ToHeader(DeckGlAttribute attribute, int offset, int amountOfVertices)
    {
        int sizeInBytes = attribute.SizeInBytesPerEntry * amountOfVertices;
        return new AttributeHeaderDto
        {
            DataType = attribute.DataType,
            SizeOfDataType = attribute.DataType.GetSize(),
            IsNormalized = attribute.IsNormalize,
            Size = attribute.Size,
            DataAccessor = attribute.DataAccessor,
            Offset = offset,
            TotalSizeInBytes = sizeInBytes
        };
    }

    public static BaseLayerHeaderDto ToHeader(this DeckGlLayer<AnnotationShape> layer, int offset,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        if (layer.LayerType == DeckGlLayerType.Composite)
        {
            return ToCompositeHeader((DeckGlCompositeLayer<AnnotationShape>) layer, offset, claimsPrincipalProvider);
        }

        return ToSingleHeader(layer, offset, claimsPrincipalProvider, false);
    }

    private static BaseLayerHeaderDto ToCompositeHeader(DeckGlCompositeLayer<AnnotationShape> layer, int offset,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        var subLayerHeaders = new List<BaseLayerHeaderDto>();

        var subHeaderOffset = 0;
        foreach (DeckGlLayer<AnnotationShape> subLayer in layer.SubLayers)
        {
            subHeaderOffset = AlignmentHelper.AlignTo4ByteOffset(subHeaderOffset);
            BaseLayerHeaderDto header = ToSingleHeader(subLayer, subHeaderOffset, claimsPrincipalProvider, true);
            subLayerHeaders.Add(header);
            subHeaderOffset += header.TotalSizeInBytes;
        }

        subHeaderOffset = AlignmentHelper.AlignTo4ByteOffset(subHeaderOffset);

        var headerDto = new CompositeLayerHeaderDto
        {
            Id = layer.Id,
            TotalSizeInBytes = subHeaderOffset,
            AmountOfEntries = layer.Length,
            Offset = offset,
            CompositeLayerHeaders = subLayerHeaders,
            CustomLayerType = layer.CustomLayerType,
            Type = DeckGlLayerType.Composite
        };

        PopulateBaseLayerHeader(headerDto, layer, claimsPrincipalProvider, false);

        return headerDto;
    }

    private static void PopulateBaseLayerHeader(BaseLayerHeaderDto header, DeckGlLayer<AnnotationShape> layer,
        IClaimsPrincipalProvider claimsPrincipalProvider, bool isSubHeader)
    {
        List<Guid> annotaIds;
        List<string> annotationDescriptions;
        List<string> annotationLabels;
        List<AnnotationType> annotationTypes;
        List<AnnotationPermissionFlags> annotationPermissionFlags;
        List<List<Guid>> annotationCounterGroupIds;
        Dictionary<Guid, List<Guid>> annotationCounterIdIndices;
        if (!isSubHeader)
        {
            var labels = new string[layer.Data.Count];
            var descriptions = new string[layer.Data.Count];
            var ids = new Guid[layer.Data.Count];
            var counterGroupIds = new List<List<Guid>>();
            var counterIdIndices = new Dictionary<Guid, List<Guid>>();
            var types = new AnnotationType[layer.Data.Count];
            var permissions = new AnnotationPermissionFlags[layer.Data.Count];
            var areSameTypes = true;
            var addedCounterGroupIds = false;
            for (var index = 0; index < layer.Data.Count; index++)
            {
                AnnotationShape annota = layer.Data[index];
                ids[index] = annota.Id;
                labels[index] = annota.Label;
                descriptions[index] = annota.Description;
                types[index] = annota.Type;
                if (annota.Type != types[0])
                {
                    areSameTypes = false;
                }

                permissions[index] = BuildAnnotationPermissionFlags(claimsPrincipalProvider, annota);

                var counterGroupIdList = new List<Guid>(annota.CounterGroups.Count);
                if (annota.CounterGroups.Count > 0)
                {
                    addedCounterGroupIds = true;
                }

                foreach (CounterGroup counterGroup in annota.CounterGroups)
                {
                    counterGroupIdList.Add(counterGroup.Id);
                    counterIdIndices[counterGroup.Id] = counterGroup.Counters.Select(x => x.Id).ToList();
                }

                counterGroupIds.Add(counterGroupIdList);
            }

            annotaIds = new List<Guid>(ids);
            annotationDescriptions = new List<string>(descriptions);
            annotationLabels = new List<string>(labels);
            annotationTypes =
                areSameTypes ? new List<AnnotationType> { types[0] } : new List<AnnotationType>(types);
            annotationPermissionFlags = new List<AnnotationPermissionFlags>(permissions);
            annotationCounterGroupIds = addedCounterGroupIds ? counterGroupIds : null;
            annotationCounterIdIndices = counterIdIndices.Count > 0 ? counterIdIndices : null;
        }
        else
        {
            annotaIds = null;
            annotationDescriptions = null;
            annotationLabels = null;
            annotationTypes = null;
            annotationPermissionFlags = null;
            annotationCounterGroupIds = null;
            annotationCounterIdIndices = null;
        }

        header.AnnotationIds = annotaIds;
        header.AnnotationDescriptions = annotationDescriptions;
        header.AnnotationLabels = annotationLabels;
        header.AnnotationTypes = annotationTypes;
        header.PermissionFlags = annotationPermissionFlags;
        header.CounterGroupIds = annotationCounterGroupIds;
        header.CounterIds = annotationCounterIdIndices;
    }

    private static AnnotationPermissionFlags BuildAnnotationPermissionFlags(
        IClaimsPrincipalProvider claimsPrincipalProvider, AnnotationShape annota)
    {
        var permissionFlags = AnnotationPermissionFlags.None;

        if (BusinessValidation.CheckUserWritePermissionBool(annota, claimsPrincipalProvider))
        {
            permissionFlags |= AnnotationPermissionFlags.CanEdit;
        }

        if (BusinessValidation.CheckUserDeletePermissionBool(annota, claimsPrincipalProvider))
        {
            permissionFlags |= AnnotationPermissionFlags.CanDelete;
        }

        if (BusinessValidation.CheckUserCanChangeVisibilityBool(annota, claimsPrincipalProvider))
        {
            permissionFlags |= AnnotationPermissionFlags.CanManageVisibility;
        }

        return permissionFlags;
    }

    private static BaseLayerHeaderDto ToSingleHeader(DeckGlLayer<AnnotationShape> layer, int offset,
        IClaimsPrincipalProvider claimsPrincipalProvider, bool isSubHeader)
    {
        Dictionary<DeckGlDataAccessor, AttributeHeaderDto> attrHeaders = MapAttributeHeaders(layer, out int totalSizeInBytes);

        if (layer.LayerType == DeckGlLayerType.Counter)
        {
            return MapCounterLayer(layer, offset);
        }

        var header = new LayerHeaderDto
        {
            Id = layer.Id,
            Type = layer.LayerType,
            AmountOfEntries = layer.Length,
            TotalSizeInBytes = totalSizeInBytes,
            Offset = offset,
            StartIndices = layer.StartIndices.IsValueCreated ? layer.StartIndices.Value.ToArray() : null,
            VertexCount = layer.VertexCount,
            AttributeHeaders = attrHeaders
        };

        PopulateBaseLayerHeader(header, layer, claimsPrincipalProvider, isSubHeader);

        return header;
    }

    private static Dictionary<DeckGlDataAccessor, AttributeHeaderDto> MapAttributeHeaders(
        DeckGlLayer<AnnotationShape> layer, out int totalSizeInBytes)
    {
        var subHeaderOffset = 0;
        var attrHeaders = new Dictionary<DeckGlDataAccessor, AttributeHeaderDto>();
        int vertexCount = layer.VertexCount;
        foreach ((DeckGlDataAccessor key, DeckGlAttribute value) in layer.Attributes)
        {
            subHeaderOffset = AlignmentHelper.AlignTo4ByteOffset(subHeaderOffset);
            AttributeHeaderDto headerDto = ToHeader(value, subHeaderOffset, vertexCount);
            attrHeaders.Add(headerDto.DataAccessor, headerDto);
            subHeaderOffset += headerDto.TotalSizeInBytes;
        }


        totalSizeInBytes = AlignmentHelper.AlignTo4ByteOffset(subHeaderOffset);

        return attrHeaders;
    }

    private static BaseLayerHeaderDto MapCounterLayer(DeckGlLayer<AnnotationShape> layer, int offset)
    {
        Dictionary<DeckGlDataAccessor, AttributeHeaderDto> attrHeaders = MapAttributeHeaders(layer, out int totalSizeInBytes);
        List<Guid> annotaIds = new();
        List<string> annotationDescriptions = new();
        List<string> annotationLabels = new();
        List<List<Guid>> annotationCounterGroupIds = new();
        Dictionary<Guid, List<Guid>> annotationCounterIdIndices = new();
        foreach (AnnotationShape annota in layer.Data)
        {
            foreach (CounterGroup group in annota.CounterGroups)
            {
                annotationDescriptions.Add(group.Description);
                annotationLabels.Add(group.Label);
                var counterIds = new List<Guid>();
                foreach (Counter counter in group.Counters)
                {
                    counterIds.Add(counter.Id);
                    annotaIds.Add(counter.Id);
                }

                annotationCounterGroupIds.Add(new List<Guid> { group.Id });
                annotationCounterIdIndices.Add(group.Id, counterIds);
            }
        }

        var header = new LayerHeaderDto
        {
            Id = layer.Id,
            Type = layer.LayerType,
            AmountOfEntries = layer.Length,
            TotalSizeInBytes = totalSizeInBytes,
            Offset = offset,
            StartIndices = layer.StartIndices.IsValueCreated ? layer.StartIndices.Value.ToArray() : null,
            VertexCount = layer.VertexCount,
            AttributeHeaders = attrHeaders,
            AnnotationDescriptions = annotationDescriptions,
            AnnotationIds = annotaIds,
            AnnotationLabels = annotationLabels,
            CounterIds = annotationCounterIdIndices,
            CounterGroupIds = annotationCounterGroupIds
        };

        return header;
    }
}