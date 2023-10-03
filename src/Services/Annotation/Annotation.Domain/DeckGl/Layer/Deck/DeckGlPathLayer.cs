using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public class DeckGlPathLayer<T> : DeckGlLayer<T>
{
    public DeckGlPathLayer(string id, bool useHighPrecision = false, byte pathSize = 2) : base(id, DeckGlLayerType.Path)
    {
        AddAttribute(DeckGlAttributeCatalog.Path(useHighPrecision, pathSize));
        AddAttribute(DeckGlAttributeCatalog.Color());
        AddAttribute(DeckGlAttributeCatalog.Width());
    }
}