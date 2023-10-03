using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlCompositeLayer<T> : DeckGlLayer<T>
{
    private readonly List<DeckGlLayer<T>> _subLayers;

    public DeckGlCompositeLayer(string id, DeckGlCustomLayerType customLayerType) : base(id, DeckGlLayerType.Composite)
    {
        CustomLayerType = customLayerType;
        _subLayers = new List<DeckGlLayer<T>>();
    }

    public DeckGlCustomLayerType CustomLayerType { get; }

    public IReadOnlyList<DeckGlLayer<T>> SubLayers => _subLayers;

    public void AddSubLayer(DeckGlLayer<T> layer)
    {
        _subLayers.Add(layer);
    }
}