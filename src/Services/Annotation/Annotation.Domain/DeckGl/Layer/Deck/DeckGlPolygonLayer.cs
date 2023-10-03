using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlPolygonLayer<T> : DeckGlLayer<T>
{
    public DeckGlPolygonLayer(string id, bool useHighPrecision = false) : base(id, DeckGlLayerType.Polygon)
    {
        AddAttribute(DeckGlAttributeCatalog.Polygon(useHighPrecision));
        AddAttribute(DeckGlAttributeCatalog.Color());
        AddAttribute(DeckGlAttributeCatalog.Width());
    }
}