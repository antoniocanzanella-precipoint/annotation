using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Attribute;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlLayer<T>
{
    private readonly Dictionary<DeckGlDataAccessor, DeckGlAttribute> _attributes;

    private int _currentIndex;

    public DeckGlLayer(string id, DeckGlLayerType layerType)
    {
        Id = id;
        LayerType = layerType;
        _attributes = new Dictionary<DeckGlDataAccessor, DeckGlAttribute>();
        Data = new List<T>();
        StartIndices = new Lazy<List<int>>();
    }

    public string Id { get; }
    public DeckGlLayerType LayerType { get; }

    /// <summary>
    /// Describes how many distinct shapes (e.g. dot, path, polygon, ...) are present in this layer.
    /// </summary>
    public virtual int Length => Data.Count;

    public Lazy<List<int>> StartIndices { get; }

    public int VertexCount
    {
        get
        {
            if (StartIndices.IsValueCreated)
            {
                return _currentIndex;
            }

            return Math.Max(_currentIndex, Length);
        }
    }

    public IReadOnlyDictionary<DeckGlDataAccessor, DeckGlAttribute> Attributes => _attributes;

    public List<T> Data { get; set; }

    public void AppendIndex(int len)
    {
        StartIndices.Value.Add(_currentIndex);
        _currentIndex += len;
    }

    protected void AddAttribute(DeckGlAttribute attr)
    {
        _attributes[attr.DataAccessor] = attr;
    }
}