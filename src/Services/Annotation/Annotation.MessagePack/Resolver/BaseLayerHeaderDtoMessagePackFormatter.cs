using MessagePack;
using MessagePack.Formatters;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.MessagePack.Resolver;

/// <summary>
/// Message pack formatter for <see cref="BaseLayerHeaderDto" />
/// </summary>
public class BaseLayerHeaderDtoMessagePackFormatter : IMessagePackFormatter<BaseLayerHeaderDto>
{
    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, BaseLayerHeaderDto value,
        MessagePackSerializerOptions options)
    {
        if (value is LayerHeaderDto layerDto)
        {
            options.Resolver.GetFormatterWithVerify<LayerHeaderDto>().Serialize(ref writer, layerDto, options);
        }
        else if (value is CompositeLayerHeaderDto compDto)
        {
            options.Resolver.GetFormatterWithVerify<CompositeLayerHeaderDto>()
                .Serialize(ref writer, compDto, options);
        }
        else
        {
            throw new MessagePackSerializationException(
                $"Unable to serialize {value.GetType()}, because its unknown!");
        }
    }

    /// <inheritdoc />
    public BaseLayerHeaderDto Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var obj =
            options.Resolver.GetFormatterWithVerify<dynamic>().Deserialize(ref reader, options) as
                Dictionary<object, object>;

        if (obj.ContainsKey(nameof(CompositeLayerHeaderDto.CompositeLayerHeaders)))
        {
            object customType = GetOrThrow(obj, nameof(CompositeLayerHeaderDto.CustomLayerType));
            var compHeadersObj = (object[]) GetOrThrow(obj, nameof(CompositeLayerHeaderDto.CompositeLayerHeaders));
            Dictionary<object, object>[] compHeadersDict = compHeadersObj.Cast<Dictionary<object, object>>().ToArray();
            List<BaseLayerHeaderDto> compHeaders = compHeadersDict.Select(ParseLayerHeader).ToList();

            var header = new CompositeLayerHeaderDto { CompositeLayerHeaders = compHeaders, CustomLayerType = (DeckGlCustomLayerType) customType };

            PopulateBaseLayerHeader(header, obj);
            return header;
        }

        return ParseLayerHeader(obj);
    }

    private void PopulateBaseLayerHeader(BaseLayerHeaderDto header, Dictionary<object, object> obj)
    {
        object offset = GetOrThrow(obj, nameof(BaseHeaderDto.Offset));
        object totalSizeInBytes = GetOrThrow(obj, nameof(BaseHeaderDto.TotalSizeInBytes));
        object id = GetOrThrow(obj, nameof(BaseLayerHeaderDto.Id));
        object type = GetOrThrow(obj, nameof(BaseLayerHeaderDto.Type));
        var annotationIds = (object[]) GetOrThrow(obj, nameof(BaseLayerHeaderDto.AnnotationIds));
        object amountOfEntries = GetOrThrow(obj, nameof(BaseLayerHeaderDto.AmountOfEntries));
        var labelObjs = (object[]) GetOrThrow(obj, nameof(BaseLayerHeaderDto.AnnotationLabels));
        var typesObjs = (object[]) GetOrThrow(obj, nameof(BaseLayerHeaderDto.AnnotationTypes));
        var permissionsObjs = (object[]) GetOrThrow(obj, nameof(BaseLayerHeaderDto.PermissionFlags));
        var descriptionObjs = (object[]) GetOrThrow(obj, nameof(BaseLayerHeaderDto.AnnotationDescriptions));
        var counterIdsObj = (Dictionary<object, object>) GetOrThrow(obj, nameof(LayerHeaderDto.CounterIds));
        object counterGroupIdsObjs = GetOrThrow(obj, nameof(LayerHeaderDto.CounterGroupIds));

        List<Guid> guids = annotationIds?.Cast<string>().Select(x => new Guid(x)).ToList();

        Dictionary<Guid, List<Guid>> counterIds = null;
        if (counterIdsObj != null)
        {
            counterIds = new Dictionary<Guid, List<Guid>>();
            foreach (KeyValuePair<object, object> x in counterIdsObj)
            {
                var key = new Guid((string) x.Key);
                counterIds.Add(key,
                    ((object[]) x.Value).Cast<string>().Select(i => new Guid(i)).ToList()
                );
            }
        }

        header.Id = (string) id;
        header.Type = (DeckGlLayerType) type;
        header.AnnotationDescriptions = descriptionObjs?.Cast<string>().ToList();
        header.AnnotationIds = guids;
        header.AnnotationLabels = labelObjs?.Cast<string>().ToList();
        header.AnnotationTypes = typesObjs?.Cast<AnnotationType>().ToList();
        header.CounterIds = counterIds;
        header.PermissionFlags = permissionsObjs?.Cast<AnnotationPermissionFlags>().ToList();
        header.AmountOfEntries = Convert.ToInt32(amountOfEntries);
        if (counterGroupIdsObjs != null)
        {
            header.CounterGroupIds = ((object[]) counterGroupIdsObjs).Cast<object[]>()
                .Select(x => x.Select(g => new Guid((string) g)).ToList()).ToList(); // can be null
        }

        header.Offset = Convert.ToInt32(offset);
        header.TotalSizeInBytes = Convert.ToInt32(totalSizeInBytes);
    }

    private BaseLayerHeaderDto ParseLayerHeader(Dictionary<object, object> obj)
    {
        var attrHeadersObj = (Dictionary<object, object>) GetOrThrow(obj, nameof(LayerHeaderDto.AttributeHeaders));
        var startIndices = (object[]) GetOrThrow(obj, nameof(LayerHeaderDto.StartIndices));
        object vertexCount = GetOrThrow(obj, nameof(LayerHeaderDto.VertexCount));

        var header = new LayerHeaderDto
        {
            AttributeHeaders = attrHeadersObj.Values
                .Select(x => ParseAttributeHeader((Dictionary<object, object>) x)).ToDictionary(x => x.DataAccessor),
            StartIndices = startIndices?.Select(Convert.ToInt32).ToArray(),
            VertexCount = Convert.ToInt32(vertexCount)
        };

        PopulateBaseLayerHeader(header, obj);

        return header;
    }

    private AttributeHeaderDto ParseAttributeHeader(Dictionary<object, object> obj)
    {
        object offset = GetOrThrow(obj, nameof(AttributeHeaderDto.Offset));
        object size = GetOrThrow(obj, nameof(AttributeHeaderDto.Size));
        object dataAccessor = GetOrThrow(obj, nameof(AttributeHeaderDto.DataAccessor));
        object dataType = GetOrThrow(obj, nameof(AttributeHeaderDto.DataType));
        object isNormalized = GetOrThrow(obj, nameof(AttributeHeaderDto.IsNormalized));
        object sizeOfDataType = GetOrThrow(obj, nameof(AttributeHeaderDto.SizeOfDataType));
        object totalSizeInBytes = GetOrThrow(obj, nameof(AttributeHeaderDto.TotalSizeInBytes));

        return new AttributeHeaderDto
        {
            Offset = Convert.ToInt32(offset),
            DataAccessor = (DeckGlDataAccessor) dataAccessor,
            Size = Convert.ToByte(size),
            DataType = (PrimitiveDataType) dataType,
            IsNormalized = Convert.ToBoolean(isNormalized),
            SizeOfDataType = Convert.ToByte(sizeOfDataType),
            TotalSizeInBytes = Convert.ToInt32(totalSizeInBytes)
        };
    }

    private object GetOrThrow(Dictionary<object, object> obj, string key)
    {
        if (!obj.TryGetValue(key, out object outObj))
        {
            throw new MessagePackSerializationException(
                $"Unable to Deserialize to {nameof(BaseLayerHeaderDto)}, because ${key} was not present!");
        }

        return outObj;
    }
}