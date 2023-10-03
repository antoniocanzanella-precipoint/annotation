using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlScatterplotLayer<T> : DeckGlLayer<T>
{
    public DeckGlScatterplotLayer(string id, bool useHighPrecision = false) : base(id, DeckGlLayerType.Scatterplot)
    {
        AddAttribute(DeckGlAttributeCatalog.Position(useHighPrecision));
        AddAttribute(DeckGlAttributeCatalog.Radius(useHighPrecision));
        AddAttribute(DeckGlAttributeCatalog.FillColor());
        AddAttribute(DeckGlAttributeCatalog.LineColor());
    }
}