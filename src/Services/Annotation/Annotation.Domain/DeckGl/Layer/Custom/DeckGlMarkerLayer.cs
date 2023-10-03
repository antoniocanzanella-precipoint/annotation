using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Custom;

public class DeckGlMarkerLayer<T> : DeckGlCompositeLayer<T>
{
    public DeckGlMarkerLayer(string id) : base(id, DeckGlCustomLayerType.Marker)
    {
        TextLayer = new DeckGlTextLayer<T>(id + "-text-layer-id");
        PathLayer = new DeckGlPathLayer<T>(id + "-path-layer-id", false, 4);
        AddSubLayer(TextLayer);
        AddSubLayer(PathLayer);
    }

    public DeckGlTextLayer<T> TextLayer { get; }

    public DeckGlPathLayer<T> PathLayer { get; }
}